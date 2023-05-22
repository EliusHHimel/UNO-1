using System;
using System.Net;

class WS
{
    static void WebServer()
    {
        // Create a new HttpListener instance
        HttpListener listener = new HttpListener();

        // Set the prefix for the server (e.g., http://localhost:8080/)
        listener.Prefixes.Add("http://localhost:8080/");

        try
        {
            // Start the listener
            listener.Start();
            Console.WriteLine("Server started. Listening for incoming requests...");

            while (true)
            {
                // Wait for an incoming request
                HttpListenerContext context = listener.GetContext();

                // Handle the request in a separate method
                HandleRequest(context);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("An error occurred: " + ex.Message);
        }
        finally
        {
            // Stop the listener when done
            listener.Stop();
        }
    }

    static void HandleRequest(HttpListenerContext context)
    {
        // Get the request object
        HttpListenerRequest request = context.Request;

        // Get the response object
        HttpListenerResponse response = context.Response;

        // Set the response content type
        response.ContentType = "text/plain";

        // Write a response to the client
        string responseString = "Hello, World!";
        byte[] buffer = System.Text.Encoding.UTF8.GetBytes(responseString);
        response.ContentLength64 = buffer.Length;
        response.OutputStream.Write(buffer, 0, buffer.Length);

        // Close the response
        response.OutputStream.Close();
    }
}
