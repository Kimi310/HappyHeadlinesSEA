using CommentService.Service.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace CommentService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CommentController : ControllerBase
{
    private readonly ICommentService _commentService;

    public CommentController(ICommentService commentService)
    {
        _commentService = commentService;
    }

    [HttpPost]
    public async Task<IActionResult> CreateComment([FromBody] CreateCommentRequest request)
    {
        try
        {
            var comment = await _commentService.CreateCommentAsync(request.ArticleId, request.CommentText);
            return Ok(comment);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("article/{articleId}")]
    public async Task<IActionResult> GetCommentsByArticleId(Guid articleId)
    {
        var comments = await _commentService.GetCommentsByArticleIdAsync(articleId);
        return Ok(comments);
    }

    [HttpDelete("{commentId}")]
    public async Task<IActionResult> DeleteComment(Guid commentId)
    {
        var deleted = await _commentService.DeleteCommentAsync(commentId);
        
        if (!deleted)
        {
            return NotFound(new { error = "Comment not found" });
        }
        
        return NoContent();
    }
}

public class CreateCommentRequest
{
    public Guid ArticleId { get; set; }
    public string CommentText { get; set; }
}

/*
 How to use:

 POST /api/comment
 {
     "articleId": "uuid",
     "commentText": "This is a great article!"
 }
 Response: Comment object with filtered content because we call profanity api in service

 GET /api/comment/article/{articleId}
 Response: Array of comments from the article id

 DELETE /api/comment/{commentId}
 Response: 204 No Content (success) or 404 Not Found
*/