using System;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;
using Microsoft.AspNetCore.Http;
using HelloSignSampleApi.Custom;
using HelloSign;
using System.Linq;

namespace HelloSignSampleApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EsignatureController : ControllerBase
    {
		private readonly IWebHostEnvironment _hostingEnvironment;

		public EsignatureController(IWebHostEnvironment hostingEnvironment)
		{
			_hostingEnvironment = hostingEnvironment;
		}

		/// <summary>
		/// Returns a list of the Templates that are accessible by you.
		/// </summary>
		[HttpGet("templates")]
        public IActionResult GetTemplates()
        {
			var apiKey = "YOUR_API_KEY";
			var client = new Client(apiKey);
			var templates = client.ListTemplates();
			return new ObjectResult(new Response<ObjectList<Template>>
			{
				Data = templates
			});
		}

		/// <summary>
		/// Returns an EditUrl that allows your site users to edit the template.
		/// </summary>
		[HttpGet("template/{id}")]
		public IActionResult GetTemplateById(string id)
		{
			var apiKey = "YOUR_API_KEY";
			var client = new Client(apiKey);
			var response = client.GetEditUrl(id, testMode:true);
			return new ObjectResult(new Response<EmbeddedTemplate>
			{
				Data = response
			});
		}

		/// <summary>
		/// Uploads document.
		/// </summary>
		[HttpPost("upload")]
        public IActionResult Upload(IFormFile file)
        {
			try
			{
				string folderName = "Upload";
				string webRootPath = _hostingEnvironment.WebRootPath;
				string newPath = Path.Combine(webRootPath, folderName);
				if (!Directory.Exists(newPath))
				{
					Directory.CreateDirectory(newPath);
				}
				if (file.Length > 0)
				{
					string fileName = ContentDispositionHeaderValue.Parse(file.ContentDisposition).FileName.Trim('"');
					string fullPath = Path.Combine(newPath, fileName);
					using (var stream = new FileStream(fullPath, FileMode.Create))
					{
						file.CopyTo(stream);
					}
					return new ObjectResult(new Response<string>
					{
						Message = "Upload Successful.",
						Data = fullPath
					});
				}
				return new ObjectResult(new Response<string>
				{
					Message = "No File"
				});
			}
			catch (Exception ex)
			{
				return new ObjectResult(new Response<string>
				{
					Message = "Upload Failed: " + ex.Message
				}); 
			}
		}

		/// <summary>
		/// Creates an embedded template draft for further editing.
		/// </summary>
		[HttpPost("create")]
		public IActionResult Create(Form form)
		{
			try
			{
				var apiKey = "YOUR_API_KEY";
				var clientId = "YOUR_CLIENT_ID";

				// create a template draft 
				var client = new Client(apiKey);
				var draft = new EmbeddedTemplateDraft();
				draft.TestMode = true;
				draft.AddFile(form.FilePath);
				draft.Title = "Test Template";
				draft.Subject = "Please sign this document";
				draft.Message = "For your approval.";
				draft.AddSignerRole("Manager", 0);
				draft.AddSignerRole("Client", 1);
				var mergeFields = form.FormFieldName.Select(item => new MergeField
				{
					Name = item.Name,
					Type = MergeField.FieldType.Text
				});
				draft.MergeFields.AddRange(mergeFields);
				//draft.MergeFields.Add(
				//	new MergeField { Name = "Name1", Type = MergeField.FieldType.Text }
				//	);
				var response = client.CreateEmbeddedTemplateDraft(draft, clientId);
				return new ObjectResult(new Response<EmbeddedTemplate>
				{
					Message = "Upload Successful.",
					Data = response
				});
				
			}
			catch (Exception ex)
			{
				return new ObjectResult(new Response<string>
				{
					Message = "Upload Failed: " + ex.Message
				});
			}
		}

		/// <summary>
		/// Creates and sends a new SignatureRequest based off of a Template.
		/// </summary>
		[HttpPost("send")]
		public IActionResult Send([FromBody] RequestSign requestData)
		{
			var apiKey = "YOUR_API_KEY";
			var clientId = "YOUR_CLIENT_ID";
			var client = new Client(apiKey);
			var request = new TemplateSignatureRequest();
			request.AddTemplate(requestData.TemplateId);
			request.AddSigner("Manager", "bob@example.com", "Bob");
			request.AddSigner("Client", "alice@example.com", "Alice");
			var customFields = requestData.CustomFields.Select(item => new CustomField
			{
				Name = item.Name,
				Value = item.Value
			});
			request.CustomFields.AddRange(customFields);
			//request.CustomFields.Add(new CustomField { Name = "Name1", Value = "Bob" });
			request.TestMode = true;
			var response = client.CreateEmbeddedSignatureRequest(request, clientId);
			return new ObjectResult(new Response<TemplateSignatureRequest>
			{
				Data = response
			});
		}

		/// <summary>
		/// Generates a temporary SignUrl using the signer's unique signature ID.
		/// </summary>
		[HttpGet("sign/{id}")]
		public IActionResult Sign(string id)
		{
			var apiKey = "YOUR_API_KEY";
			var client = new Client(apiKey);
			var response = client.GetSignUrl(id);
			return new ObjectResult(new Response<EmbeddedSign>
			{
				Data = response
			});
		}

		/// <summary>
		/// Obtain a copy of the current documents.
		/// </summary>
		[HttpGet("view/{id}")]
		public IActionResult View(string id)
		{
			var apiKey = "YOUR_API_KEY";
			var client = new Client(apiKey);
			var response = client.DownloadSignatureRequestFiles(id);
			return new FileContentResult(response, "application/octet");
		}

		/// <summary>
		/// Lists the SignatureRequests (both inbound and outbound) that you have access to.
		/// </summary>
		[HttpGet("signatureRequests")]
		public IActionResult GetSignatureRequests()
		{
			var apiKey = "YOUR_API_KEY";
			var client = new Client(apiKey);
			var response = client.ListSignatureRequests();
			return new ObjectResult(new Response<ObjectList<SignatureRequest>>
			{
				Data = response
			});
		}
	}
}
