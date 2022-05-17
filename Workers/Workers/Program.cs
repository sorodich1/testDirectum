using ConsoleTables;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Workers
{
    internal class Program
    {
		private static SqlConnection sqlConnection = null;
		static void Main(string[] args)
        {
			sqlConnection = new SqlConnection(ConfigurationManager.ConnectionStrings["ConnectionDb"].ConnectionString);

			sqlConnection.Open();

			if (sqlConnection.State == ConnectionState.Open)
			{
				Console.WriteLine("Подключение установленно\n");

				Console.WriteLine("Список всех сотрудников\n");

				SqlCommand sql1 = new SqlCommand(
					$"SELECT employee.name, department.name, employee.salary FROM employee INNER JOIN department on department.id = employee.department_id "
					, sqlConnection);

				using (SqlDataReader reader = sql1.ExecuteReader())
				{
					while (reader.Read())
					{
						Console.WriteLine(String.Format("|{0,10}|{1,10}|{2,10}|", reader[0], reader[1], reader[2]));
					}
				}

				Console.WriteLine("\n Суммарная зарплата в разрезе департаментов\n");

				SqlCommand withoutLead = new SqlCommand(
					$"SELECT department.name, SUM(employee.salary) AS total FROM employee INNER JOIN department ON employee.department_id = department.id GROUP BY department.name; "
					, sqlConnection);
				Console.WriteLine("\n Без учёта зарплаты руководителя\n");
				using (SqlDataReader reader = withoutLead.ExecuteReader())
				{
					while (reader.Read())
					{
						Console.WriteLine(String.Format("|{0,10}|{1,10}|", reader[0], reader[1]));
					}
				}

				SqlCommand withLead = new SqlCommand(
					$"with a as (SELECT department_id, chief_id, SUM(employee.salary) AS total FROM employee WHERE chief_id is not null GROUP BY department_id, chief_id) select department.name, salary + total as total FROM a INNER JOIN department ON a.department_id = department.id inner join employee as e on a.chief_id = e.id "
					, sqlConnection);
				Console.WriteLine("\n Учитывая зарплату руководителя\n");
				using (SqlDataReader reader = withLead.ExecuteReader())
				{
					while (reader.Read())
					{
						Console.WriteLine(String.Format("|{0,10}|{1,10}|", reader[0], reader[1]));
					}
				}

				Console.WriteLine();

				SqlCommand departmentMaxSum = new SqlCommand(
					$"SELECT department.name, employee.name FROM employee INNER JOIN department ON employee.department_id = department.id WHERE employee.salary = (SELECT MAX(employee.salary) FROM employee);"
					, sqlConnection);

				using (SqlDataReader reader = departmentMaxSum.ExecuteReader())
				{
					while (reader.Read())
					{
						Console.WriteLine(String.Format(" \nМаксимальная зарплата в департаменте {0}, у сотрудника {1}\n", reader[0], reader[1]));
					}
				}

				Console.WriteLine();

				Console.WriteLine("\nЗарплаты руководителей департаментов (по убыванию)\n");
				Console.WriteLine();

				SqlCommand salaryLead = new SqlCommand(
					$"SELECT name, salary FROM employee WHERE department_id = (SELECT MAX(department_id) FROM employee) ORDER BY salary DESC"
					, sqlConnection);

				using (SqlDataReader reader = salaryLead.ExecuteReader())
				{
					while (reader.Read())
					{
						Console.WriteLine(String.Format("|{0,10}|{1,10}|", reader[0], reader[1]));
					}
				}
			}

			Console.ReadLine();
		}
    }
}
