#:package WebPush@1.0.13

// Run locally once: dotnet run tools/GenerateVapidKeys.cs
// Redirect output to a private environment file; never commit that file.

var keys = WebPush.VapidHelper.GenerateVapidKeys();

Console.WriteLine(
    $"Push__PublicKey={keys.PublicKey}");

Console.WriteLine(
    $"Push__PrivateKey={keys.PrivateKey}");