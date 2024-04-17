using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;

namespace WindowsForms
{
    interface IView
    {
        string FirstDirectory();
        string SecondDirectory();

        void Synchronize(List<string> messages);

        event EventHandler<EventArgs> SynchronizeDirectories;
    }

    class Model
    {
        public List<string> SynchronizingDirectories(string firstDirectory,  string secondDirectory)
        {
            DirectoryInfo sourceDirectory = new DirectoryInfo(firstDirectory);
            DirectoryInfo destinationDirectory = new DirectoryInfo(secondDirectory);

            List<string> synchronizationResult;

            synchronizationResult = InternalDirectorySynchronization(sourceDirectory, destinationDirectory);

            return synchronizationResult;
        }

        private List<string> InternalDirectorySynchronization(DirectoryInfo firstDirectory, DirectoryInfo secondDirectory)
        {
            List<string> resultOfInternalDirectorySynchronization = new List<string>();

            foreach (FileInfo file in firstDirectory.GetFiles())
            {
                FileInfo fileInFirstDirectory = new FileInfo(Path.Combine(secondDirectory.FullName, file.Name));

                if (!fileInFirstDirectory.Exists || fileInFirstDirectory.LastWriteTime < file.LastWriteTime)
                {
                    File.Copy(file.FullName, fileInFirstDirectory.FullName, true);
                    resultOfInternalDirectorySynchronization.Add($"Файл {file.Name} изменен");
                }
            }

            foreach (FileInfo file in secondDirectory.GetFiles())
            {
                FileInfo fileInSecondDirectory = new FileInfo(Path.Combine(firstDirectory.FullName, file.Name));

                if (!fileInSecondDirectory.Exists)
                {
                    file.Delete();
                    resultOfInternalDirectorySynchronization.Add($"Файл {file.Name} удален");
                }
            }
            return resultOfInternalDirectorySynchronization;
        }
    }

    class Presenter
    {
        private IView view;
        private Model model;
        public Presenter(IView newView)
        {
            view = newView;
            model = new Model();

            view.SynchronizeDirectories += new EventHandler<EventArgs>(Synchronize);
        }

        private void Synchronize(object sender, EventArgs newEvent)
        {
            List<string> resultOfSynchronization = model.SynchronizingDirectories(view.FirstDirectory(), view.SecondDirectory());
            view.Synchronize(resultOfSynchronization);
        }
    }

    class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new View());
        }
    }
}
