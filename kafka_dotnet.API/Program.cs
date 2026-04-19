using System.Text.Json;
using Confluent.Kafka;
using Confluent.Kafka.Admin;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

//app.UseHttpsRedirection();

var env = builder.Environment.EnvironmentName;

builder.Configuration
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{env}.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables();


 string topicName = app.Configuration["kafka:topicName"]!;
 string bootstrapServers = app.Configuration["kafka:BootstrapServers"]!;

app.MapPost("/orders/create", async ([FromBody]  Order order) =>
    {
        var config = new AdminClientConfig
        {
            BootstrapServers = bootstrapServers
        };

        using var adminClient = new AdminClientBuilder(config).Build();
        var metaDatas = adminClient.GetMetadata(TimeSpan.FromSeconds(10));
        var found = metaDatas.Topics.Any(topic => topic.Topic.Equals(topicName));
        if (!found)
        {
            await adminClient.CreateTopicsAsync(new TopicSpecification[]
            {
                new TopicSpecification
                {
                    Name = topicName,
                    NumPartitions = 1,
                    ReplicationFactor = 1
                }
            });
        }

        using (var producer = new ProducerBuilder<Null, string>(config).Build())
        {
            try
            {

                var jsonString = JsonSerializer.Serialize(order);
                var delReport = await producer.ProduceAsync(topicName, new Message<Null, string> { Value = jsonString });

            }
            catch (ProduceException<Null, string> e)
            {
                return Results.BadRequest(e.Error.Reason);
            }
        }

        return Results.Ok($"orders  created successfully Env : {env}");
    })
    .WithName("GetOrders");

    app.MapGet("/orders/recieve", async () => 
    {
        var config = new ConsumerConfig
        {
            BootstrapServers = bootstrapServers,
           GroupId = "consumer-group",
           AutoOffsetReset =  AutoOffsetReset.Earliest
        };
        var topicName = "create-order";
        using (var consumer = new ConsumerBuilder<Ignore, string>(config).Build())
        {
            consumer.Subscribe(topicName);
            CancellationTokenSource cts = new CancellationTokenSource();
            Console.CancelKeyPress += (_, e) =>
            {
                e.Cancel = true;
                cts.Cancel();
            };

            try
            {
                while (true)
                {
                    try
                    {
                        var consumeResult = consumer.Consume(cts.Token);
                        return Results.Ok(consumeResult.Message.Value);
                    }
                    catch (ConsumeException e)
                    {
                        return Results.BadRequest(e.Message);
                    }
                }
            }
            catch (OperationCanceledException e)
            {
               consumer.Close();
               return Results.BadRequest(e.Message);
            }

        }
    })
    .WithName("recieveOrder");

    app.MapPost("/topics/createTopic", async ([FromBody] topicRequest request) =>
    {
        var config = new AdminClientConfig
        {
            BootstrapServers = bootstrapServers
        };
        using var adminClient = new AdminClientBuilder(config).Build();
        var metaDatas = adminClient.GetMetadata(TimeSpan.FromSeconds(10));
        var found = metaDatas.Topics.Any(topic => topic.Topic.Equals(request.topicName));
        if (found)
            return Results.BadRequest("Topic already exists");

        await adminClient.CreateTopicsAsync(new TopicSpecification[]
        {
            new TopicSpecification
            {
                Name = request.topicName,
                NumPartitions = 1,
                ReplicationFactor = 1
            }
        });
        return Results.Ok($"Topic '{request.topicName}' created successfully.");

    }).WithName("createTopic");

app.Run();
record topicRequest(string topicName);
record Order(int Id, string ProductName, int Quantity);