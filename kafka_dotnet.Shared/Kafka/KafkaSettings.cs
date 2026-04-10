public sealed record KafkaSettings
{
    public string   BootstrapServers   { get; init; } = default!;
    public string   SchemaRegistryUrl  { get; init; } = default!;
    public string?  SaslUsername       { get; init; }
    public string?  SaslPassword       { get; init; }
    public string   SecurityProtocol   { get; init; } = "Plaintext";
    public string   SaslMechanism      { get; init; } = "Plain";
    public string   ProducerAcks       { get; init; } = "All";
    public string   ConsumerGroupId    { get; init; } = default!;
    public string   AutoOffsetReset    { get; init; } = "Earliest";
}