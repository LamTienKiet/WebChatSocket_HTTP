using System.Text.Json;
using System.Text.Json.Serialization;
using WebSocketTest.Models;
using WebSocketTest.Data;
namespace WebSocketTest.Data
{
    public static class AuthService
    {
        public static bool Validate(string username, string password)
            => UserJsonReader.Instance.Validate(username, password);
    }
}