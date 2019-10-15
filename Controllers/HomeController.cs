using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using WeddingPlanner.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

namespace WeddingPlanner.Controllers
{
    public class HomeController : Controller
    {
        private WeddingPlannerContext dbContext;

        public HomeController(WeddingPlannerContext context)
        {
            dbContext = context;
        }

        // *************************************************************************************************************************

        // Login & Register Page
        [HttpGet("")]
        public IActionResult Index()
        {
            return View("Index");
        }

        // Register User
        [HttpPost("register")]
        public IActionResult RegisterUser(User newUser)
        {
            if(ModelState.IsValid)
            {
                if(dbContext.Users.Any(u => u.Email == newUser.Email))
                {
                    ModelState.AddModelError("Email", "Email is already in use!");
                    return View("Index");
                }
                PasswordHasher<User> Hasher = new PasswordHasher<User>();
                newUser.Password = Hasher.HashPassword(newUser, newUser.Password);
                dbContext.Add(newUser);
                dbContext.SaveChanges();
                HttpContext.Session.SetString("LoginUserEmail", newUser.Email);
                HttpContext.Session.SetInt32("CurUserId", newUser.UserId);
                return RedirectToAction("Dashboard");
            }
            return View("Index");
        }

        // Login User
        [HttpPost("login")]
        public IActionResult LoginUser(LoginUser userSubmission)
        {
            if(ModelState.IsValid)
            {
                var userInDb = dbContext.Users.FirstOrDefault(u => u.Email == userSubmission.Email);
                if(userInDb == null)
                {
                    ModelState.AddModelError("Email", "Invalid Email/Password");
                    return View("Index");
                }
                var hasher = new PasswordHasher<LoginUser>();
                var result = hasher.VerifyHashedPassword(userSubmission, userInDb.Password, userSubmission.Password);
                if(result == 0)
                {
                    ModelState.AddModelError("Password", "Password is incorrect, please try again.");
                    return View("Index");
                }
                HttpContext.Session.SetString("LoginUserEmail", userSubmission.Email);
                HttpContext.Session.SetInt32("CurUserId", userInDb.UserId);
                return RedirectToAction("Dashboard");
            }
            return View("Index");
        }

        // Dashboard - show all weddings
        [HttpGet("dashboard")]
        public IActionResult Dashboard()
        {
            if(HttpContext.Session.GetInt32("CurUserId") == null)
            {
                return RedirectToAction("Index");
            }
            int curUser = (int)HttpContext.Session.GetInt32("CurUserId");
            User user = dbContext.Users.Include(u => u.CreatedWeddings)
                .Include(u => u.Attending)
                .ThenInclude(u => u.Wedding)
                .FirstOrDefault(u => u.UserId == curUser);
            ViewBag.user = user;
            List<Wedding> AllWeddings = dbContext.Weddings
                .Include(w => w.Atendees)
                .ToList();
            List<Wedding> attending = new List<Wedding>();
            List<Wedding> notAttending = new List<Wedding>();
            foreach(Attendee attend in user.Attending)
            {
                attending.Add(attend.Wedding);
            }
            foreach(Wedding wedding in AllWeddings)
            {
                if(!attending.Contains(wedding))
                {
                    notAttending.Add(wedding);
                }
            }
            ViewBag.notAttending = notAttending;
            // use logic from tri????????????????????????????????????????????????????
            return View("Dashboard", AllWeddings);
        }

        // Plan wedding page
        [HttpGet("wedding/new")]
        public IActionResult Wedding()
        {
            if(HttpContext.Session.GetInt32("CurUserId") == null)
            {
                return RedirectToAction("Index");
            }
            return View("Wedding");
        }

        // Create a wedding event
        [HttpPost("wedding/create")]
        public IActionResult CreateWedding(Wedding newWedding)
        {
            newWedding.UserId = (int)HttpContext.Session.GetInt32("CurUserId");
            if(ModelState.IsValid)
            {
                dbContext.Add(newWedding);
                dbContext.SaveChanges();
                return RedirectToAction("Dashboard");
            }
            return View("Wedding");
        }

        // Details of a particular wedding
        [HttpGet("details/{weddingId}")]
        public IActionResult OneWedding(int weddingId)
        {
            if(HttpContext.Session.GetInt32("CurUserId") == null)
            {
                return RedirectToAction("Index");
            }
            if(!dbContext.Weddings.Any(u => u.WeddingId == weddingId))
            {
                return RedirectToAction("Dashboard");
            }
            Wedding oneWedding = dbContext.Weddings
                .Include(w => w.Atendees)
                .ThenInclude(a => a.User)
                .FirstOrDefault(w => w.WeddingId == weddingId);
            return View("Details", oneWedding);
        }

        // Logout
        [HttpGet("logout")]
        public IActionResult LogoutUser()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index");
        }

        // Delete wedding
        [HttpGet("delete/{weddingId}")]
        public IActionResult DeleteWedding(int weddingId)
        {
            if(HttpContext.Session.GetInt32("CurUserId") == null)
            {
                return RedirectToAction("Index");
            }
            if(!dbContext.Weddings.Any(u => u.WeddingId == weddingId))
            {
                return RedirectToAction("Dashboard");
            }
            Wedding deleteWedding = dbContext.Weddings.FirstOrDefault(w => w.WeddingId == weddingId);
            int curUser = (int)HttpContext.Session.GetInt32("CurUserId");
            if(deleteWedding.UserId != curUser)
            {
                return RedirectToAction("Dashboard");
            }
            dbContext.Weddings.Remove(deleteWedding);
            dbContext.SaveChanges();
            return RedirectToAction("Dashboard");
        }
        
        // RSVP
        [HttpGet("rsvp/{weddingId}")]
        public IActionResult ReserveSpot(int weddingId)
        {
            if(HttpContext.Session.GetInt32("CurUserId") == null)
            {
                return RedirectToAction("Index");
            }
            if(!dbContext.Weddings.Any(u => u.WeddingId == weddingId))
            {
                return RedirectToAction("Dashboard");
            }
            int curUser = (int)HttpContext.Session.GetInt32("CurUserId");
            Attendee selectedAttendee = new Attendee();
            selectedAttendee.UserId = curUser;
            selectedAttendee.WeddingId = weddingId;
            dbContext.Add(selectedAttendee);
            dbContext.SaveChanges(); 
            return RedirectToAction("Dashboard");
        }

        // un-RSVP
        [HttpGet("cancel/{weddingId}")]
        public IActionResult CancelReservedSpot(int weddingId)
        {
            if(HttpContext.Session.GetInt32("CurUserId") == null)
            {
                return RedirectToAction("Index");
            }
            if(!dbContext.Weddings.Any(u => u.WeddingId == weddingId))
            {
                return RedirectToAction("Dashboard");
            }
            int curUser = (int)HttpContext.Session.GetInt32("CurUserId");
            Attendee selectedAttendee = dbContext.Attendees.FirstOrDefault(a => a.WeddingId == weddingId && a.UserId == curUser);
            if(selectedAttendee.Equals(default(Attendee)))
            {
                return RedirectToAction("Dashboard");
            }
            dbContext.Attendees.Remove(selectedAttendee);
            dbContext.SaveChanges();
            return RedirectToAction("Dashboard");
        }

        // *************************************************************************************************************************

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
