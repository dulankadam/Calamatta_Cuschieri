using Application.Interfaces;
using Domain.Models;
using Infrastructure.Repositories;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

[ApiController]
[Route("api/[controller]")]
public class ChatsController : ControllerBase
{
    private readonly IQueueService _queueService;
    private readonly InMemoryAgentRepository _agentRepository;

    public ChatsController(IQueueService queueService, InMemoryAgentRepository agentRepository)
    {
        _queueService = queueService;
        _agentRepository = agentRepository;
    }

    [HttpPost]
    public async Task<IActionResult> CreateChat()
    {
        var chat = new ChatSession();
        var result = await _queueService.EnqueueAsync(chat);

        if (result.Refused)
            return StatusCode(429, new { message = "Queue is full, chat refused." });

        return Ok(result);
    }

    [HttpPost("{id:guid}/poll")]
    public IActionResult PollChat(Guid id)
    {
        _queueService.MarkPolled(id);
        var chat = _queueService.Get(id);

        if (chat == null) return NotFound();

        return Ok(new
        {
            assigned = chat.IsAssigned,
            agentId = chat.AssignedAgentId
        });
    }

    [HttpGet("agents")]
    public IActionResult GetAgents() => Ok(_agentRepository.GetAll());
}
