﻿namespace SharedLibrary
{
    public class Response
    {
        public bool Success { get; set; }
        public string Message { get; set; }

        public static Response Succeed(string message = "")
        {
            return new Response { Success = true, Message = message };
        }

        public static Response Fail(string message="")
        {
            return new Response { Success = false, Message = message };
        }
    }
}
