using System;

namespace IndependentWork1
{
    public class Student
    {
        private string _name;
        private double _averageGrade;

        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }

        public double AverageGrade
        {
            get { return _averageGrade; }
        }

        public Student(string name, double averageGrade)
        {
            _name = name;
            _averageGrade = averageGrade;
        }

        public bool HasScholarship()
        {
            return _averageGrade >= 4.5;
        }
    }

    public class Course
    {
        private string _title;
        private int _hours;

        public string Title
        {
            get { return _title; }
        }

        public int Hours
        {
            get { return _hours; }
            set { _hours = value; }
        }

        public Course(string title, int hours)
        {
            _title = title;
            _hours = hours;
        }

        public double GetWeeksDuration(int hoursPerWeek)
        {
            return (double)_hours / hoursPerWeek;
        }
    }

    public class Program
    {
        public static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;

            Student student = new Student("Олексій", 4.8);
            Console.WriteLine($"Студент: {student.Name}");
            Console.WriteLine($"Середній бал: {student.AverageGrade}");
            Console.WriteLine($"Чи є стипендія: {(student.HasScholarship() ? "Так" : "Ні")}");

            Console.WriteLine();

            Course course = new Course("Об'єктно-орієнтоване програмування", 120);
            Console.WriteLine($"Предмет: {course.Title}");
            Console.WriteLine($"Всього годин: {course.Hours}");
            Console.WriteLine($"Кількість тижнів (по 10 год/тиж): {course.GetWeeksDuration(10)}");
        }
    }
}