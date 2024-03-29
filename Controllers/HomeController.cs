﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.EntityFrameworkCore.ValueGeneration.Internal;
using System.Diagnostics;
using ToDoApp.Models;

namespace ToDoApp.Controllers
{
	public class HomeController : Controller
	{
		private ToDoContext context;

		// Constructor for the controller that injects a ToDoContext.
		public HomeController(ToDoContext ctx) => context = ctx;

		// Action method to display the list of tasks.
		public IActionResult Index(string id)
		{
			// Create a Filters object based on the input 'id'.
			var filters = new Filters(id);
			ViewBag.Filters = filters;
			ViewBag.Statuses = context.Statuses.ToList();
			ViewBag.DueFilters = Filters.DueFilterValues;

			// Create a query to retrieve To-Do items including related status.
			IQueryable<ToDo> query = context.ToDos
				.Include(t => t.status);

			// Apply filters to the query based on user input.
			if (filters.HasStatus) query = query.Where(t => t.StatusId == filters.StatusId);
			if (filters.HasDue)
			{
				var today = DateTime.Today;
				if (filters.IsPast) query = query.Where(t => t.DueDate < today);
				else if (filters.IsFuture) query = query.Where(t => t.DueDate > today);
				else if (filters.IsToday) query = query.Where(t => t.DueDate == today);
			}

			// Retrieve tasks, order them by due date, and convert to a list.
			var tasks = query.OrderBy(t => t.DueDate).ToList();

			return View(tasks);
		}

		// Action method to add a new task (HTTP GET).
		[HttpGet]
		public IActionResult Add()
		{
			ViewBag.Statuses = context.Statuses.ToList();
			var task = new ToDo { StatusId = "open" };
			return View(task);
		}

		// Action method to add a new task (HTTP POST).
		[HttpPost]
		public IActionResult Add(ToDo task)
		{
			if (ModelState.IsValid)
			{
				context.ToDos.Add(task);
				context.SaveChanges();
				return RedirectToAction("Index");
			}
			else
			{
				ViewBag.Statuses = context.Statuses.ToList();
				return View(task);
			}
		}
		public async Task<ToDo> GetTaskByIdAsync(int id)
        {
            return await context.ToDos.FindAsync(id);
        }
		// Action method to filter tasks based on user input.
		[HttpPost]
		public IActionResult Filter(string[] filter)
		{
			string id = string.Join('-', filter);
			return RedirectToAction("Index", new { ID = id });
		}

		// Action method to mark a task as complete.
		[HttpPost]
		public IActionResult MarkComplete([FromRoute] string id, ToDo selected)
		{
			selected = context.ToDos.Find(selected.Id)!;
			if (selected != null)
			{
				selected.StatusId = "closed";
				context.SaveChanges();
			}
			return RedirectToAction("Index", new { ID = id });
		}

		// Action method to delete completed tasks.
		public IActionResult DeleteComplete(string id)
		{
			var toDelete = context.ToDos.Where(t => t.StatusId == "closed").ToList();

			foreach (var task in toDelete) context.ToDos.Remove(task);
			context.SaveChanges();

			return RedirectToAction("Index", new { ID = id });
		}

		// Action method to handle errors.
		[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
		public IActionResult Error()
		{
			return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
		}
		
		[HttpGet]
    public async Task<IActionResult> DeleteTaskAsync(int id)
        {
            var taskToDelete = await context.ToDos.FindAsync(id);
            if (taskToDelete != null)
            {
                context.ToDos.Remove(taskToDelete);
                await context.SaveChangesAsync();
				return Ok();
            }
			return NotFound();
        }

    [HttpGet]
    public async Task<IActionResult> Delete(int id)
    {
        await DeleteTaskAsync(id);
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> EditTitle(int id)
    {
        var task = await GetTaskByIdAsync(id);
        return View(task);
    }
	 public async Task UpdateTaskAsync(ToDo task)
        {
            context.Entry(task).State = EntityState.Modified;
            await context.SaveChangesAsync();
        }

	[HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var task = await GetTaskByIdAsync(id);
        return View(task);
    }

	[HttpPost]
    public async Task<IActionResult> Edit(ToDo taskDto)
    {
        if (ModelState.IsValid)
        {
            await UpdateTaskAsync(taskDto);
          
        }  
		return RedirectToAction(nameof(Index));
    }


    [HttpPost]
    public async Task<IActionResult> EditTitle(ToDo taskDto)
    {
        if (ModelState.IsValid)
        {
            await UpdateTaskAsync(taskDto);
            return RedirectToAction(nameof(Index));
        }
        return View(taskDto);
    }  
	}
}

