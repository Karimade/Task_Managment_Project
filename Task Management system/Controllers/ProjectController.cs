using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Task_Management_system.Data.UnitOfWork;
using Task_Management_system.Models;

namespace Task_Management_system.Controllers
{
    [Authorize]
    public class ProjectController : Controller
    {
        private readonly IUnitOfWork _UOW;

        public ProjectController(IUnitOfWork UOW)
        {
            _UOW = UOW;
        }
        public ActionResult Index ()
        {
            IEnumerable<Project> projects = _UOW.projects.GetAll() ;
            return View(projects);
        }

        [HttpGet]
        public ActionResult Insert()
        {
            return View();
        } 
        [HttpPost]
        public ActionResult Insert(Project project) 
        {
            if (ModelState.IsValid)
            {
                _UOW.projects.Add(project);
                _UOW.SaveChanges();
                return RedirectToAction("Index");
            }
            return View();
        }

        [HttpGet]
        public ActionResult Edit(int id)
        {
            var tbEditedproject = _UOW.projects.GetBy(id);
            return View(tbEditedproject);
        }
        [HttpPost]
        public ActionResult Edit(Project editedProject)
        {
            if (ModelState.IsValid)
            {
                _UOW.projects.Update(editedProject);
                _UOW.SaveChanges();
                return RedirectToAction("Index");
            }
            return View();
        }
        public ActionResult Delete(int id)
        {
            _UOW.projects.Delete(id);
            _UOW.SaveChanges();
            return RedirectToAction("Index");
        }

       
    }
}
