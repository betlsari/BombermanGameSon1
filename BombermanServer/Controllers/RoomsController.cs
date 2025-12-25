using Microsoft.AspNetCore.Mvc;
using BombermanServer.Services;
using BombermanServer.Controllers;
using BombermanServer.Hubs;
using BombermanServer.Models.Enums;
using BombermanServer.Models;
using Microsoft.AspNetCore.SignalR;
using System.Reflection;

namespace BombermanServer.Controllers
{
	[ApiController]
	[Route("api/[controller]")]
	public class RoomsController : ControllerBase
	{
		private readonly IRoomService _roomService;

		public RoomsController(IRoomService roomService)
		{
			_roomService = roomService;
		}

		[HttpGet]
		public IActionResult GetRooms()
		{
			var rooms = _roomService.GetAllRooms();
			return Ok(new { rooms, count = rooms.Count });
		}

		[HttpGet("{roomId}")]
		public IActionResult GetRoom(string roomId)
		{
			var room = _roomService.GetRoom(roomId);
			if (room == null)
			{
				return NotFound();
			}
			return Ok(room);
		}

		[HttpGet("health")]
		public IActionResult Health()
		{
			return Ok(new
			{
				status = "healthy",
				timestamp = DateTime.UtcNow,
				activeRooms = _roomService.GetAllRooms().Count
			});
		}
	}
}