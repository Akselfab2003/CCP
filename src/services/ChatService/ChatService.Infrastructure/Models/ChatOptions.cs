using System;
using System.Collections.Generic;
using System.Text;

namespace ChatService.Models;

public class ChatOptions
{
    public string ChatModel { get; set; } = "llama3";
    public string EmbeddingModel { get; set; } = "nomic-embed-text";
    public int TopKResults { get; set; } = 3;
    public double SimilarityThreshold { get; set; } = 0.75;
}
