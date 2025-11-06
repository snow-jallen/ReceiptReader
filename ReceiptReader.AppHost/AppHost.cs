var builder = DistributedApplication.CreateBuilder(args);

// Add connection string reference for existing Azure OpenAI resource
// This uses your existing Azure OpenAI instance without provisioning new resources
var openai = builder.AddConnectionString("openai");

builder.AddProject<Projects.ReceiptReader>("receiptreader")
    .WithReference(openai);

builder.Build().Run();
