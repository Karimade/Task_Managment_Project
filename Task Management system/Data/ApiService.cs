using System.Net.Http.Json;
using System.Text.Json;
using Task = Task_Management_system.Models.Task;
using Newtonsoft.Json;
namespace Task_Management_system.Data;

// This class used for interacting with the API, Getting the data from it , Deserializing it
    public class ApiService
    {
        //Used for making requests to the api
        private readonly HttpClient _httpClient;
        public ApiService(HttpClient httpClient)
        {
            _httpClient = httpClient;
            _httpClient.BaseAddress = new Uri("https://localhost:7240/");
    }


    public IEnumerable<Task> GetTasksFiltered(int? projectId, int? userId, DateTime? dueDateFrom, DateTime? dueDateTo)
    {
        string apiUrl = $"api/TaskApi?projectId={projectId}&userId={userId}&dueDateFrom={dueDateFrom}&dueDateTo={dueDateTo}";
        var response = _httpClient.GetAsync(apiUrl).Result;
        if (response.IsSuccessStatusCode)
        {
            
            var content = response.Content.ReadAsStringAsync().Result;
            Console.WriteLine("API Response: " + content); 

            // Attempt deserialization
            if (content != null)
            {
                var deserialized = JsonConvert.DeserializeObject<List<Task>>(content);
                return deserialized.ToList();
               // return System.Text.Json.JsonSerializer.Deserialize<IEnumerable<Task>>(content);
                
            }
        }
        return Enumerable.Empty<Task>();
    }



    public Task GetTaskDetails(int taskId)
        {
            var response = _httpClient.GetAsync($"api/TaskApi/{taskId}").Result;

        if (response.IsSuccessStatusCode)
        {
            var content = response.Content.ReadAsStringAsync().Result;
            
            if (content != null)
            {
                var deserialized = JsonConvert.DeserializeObject<Task>(content);
                return deserialized;
                
            }
        }
            return new Task();
        }
      
            
       
        public IEnumerable<Task> GetOverdueTasks(int count)
        {
            var response = _httpClient.GetAsync($"api/TaskApi/overdue/{count}").Result;

            if (response.IsSuccessStatusCode)
            {
                return response.Content.ReadFromJsonAsync<IEnumerable<Task>>().Result;
            }
            return Enumerable.Empty<Task>();
        }
    }

