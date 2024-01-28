using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace ToDoApp.Models
{
	/// <summary>
	/// Represents a To-Do item with various properties for tracking tasks.
	/// </summary>
	public class ToDo
	{
		/// Gets or sets the unique identifier of the To-Do item.
		public int Id { get; set; }

		/// Gets or sets the description of the To-Do item.
		[Required]
    	[MaxLength(255)]
    	public string Title { get; set; }
		[Required(ErrorMessage = "Please enter a description.")]
		public string Description { get; set; } = string.Empty;

		/// <summary>
		/// Gets or sets the due date for the To-Do item.
		/// </summary>
		[Required(ErrorMessage = "Please enter a due date.")]
		public DateTime? DueDate { get; set; }
		/// Gets or sets the status identifier for the To-Do item.
		[Required(ErrorMessage = "Please select a status.")]
		public string StatusId { get; set; } = string.Empty;

		/// Gets or sets the Status object associated with the To-Do item.
		[ValidateNever]
		public Status status { get; set; } = null!;
		/// Gets a boolean value indicating whether the To-Do item is overdue.
		public bool Overdue => StatusId == "open" && DueDate < DateTime.Today;
	}

}
