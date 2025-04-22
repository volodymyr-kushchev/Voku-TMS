// © 2025 Volodymyr Kushchev. Use of this code is restricted to evaluation purposes only.
// Contact: volodymyr.kushchev@gmail.com

namespace TaskManagement.Application.DTOs;

/// <summary>
/// Request model for paginated results
/// </summary>
public class PaginationRequest
{
    /// <summary>
    /// The page number (1-based)
    /// </summary>
    /// <example>1</example>
    public int? PageNumber { get; set; }

    /// <summary>
    /// The number of items per page
    /// </summary>
    /// <example>10</example>
    public int? PageSize { get; set; }
} 