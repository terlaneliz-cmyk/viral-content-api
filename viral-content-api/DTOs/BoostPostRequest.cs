using System.ComponentModel.DataAnnotations;

namespace ViralContentApi.DTOs;

public class BoostPostRequest
{
    [Range(0.1, double.MaxValue)]
    public double Amount { get; set; }
}