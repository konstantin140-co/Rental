using Microsoft.EntityFrameworkCore;
using Rental.Data.Context;
using Rental.Forms.Forms;

namespace Rental.Forms;

static class Program
{
    [STAThread]
    static void Main()
    {
        ApplicationConfiguration.Initialize();

        using var db = new AppDbContext();
        db.Database.Migrate();

        Application.Run(new MainForm());
    }
}
