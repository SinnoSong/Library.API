﻿using Newtonsoft.Json;

namespace Library.Common.Models;

public class NoticeCreateDto
{
    public NoticeCreateDto(string title, string content)
    {
        Title = title;
        Content = content;
    }

    [JsonProperty("title", Required = Required.Always)]
    public string Title { get; set; }

    [JsonProperty("content", Required = Required.Always)]
    public string Content { get; set; }
}