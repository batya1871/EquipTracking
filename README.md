# EquipTracking
## Overview
**EquipTracking** is a C# WPF desktop application designed for tracking agricultural machinery, its operational status, and related events. The program allows you to manage data about machinery, such as work hours, fuel levels, technical condition, and location, as well as track events with the machinery and save/load this data.

## Key Features

- **Machinery Status Tracking**: The application provides information about the machinery's operation, location, and fuel consumption.
- **Save and Load Data**: Machinery and event data are saved in JSON files for long-term availability.
- **Notifications**: The notification system alerts the user about various events related to the machinery.
---
## Project Structure

1. **AgrMachinery.cs** — A class representing agricultural machinery (e.g., tractors or combine harvesters).
   - Stores machinery information: operator, location, type, working hours, fuel status, and technical condition.
   - Includes methods for checking machinery's operational status, calculating work time, and fuel consumption.

2. **SaveAndLoadService.cs** — A service for loading and saving data.
   - Loads the list of machinery and the event list from JSON files.
   - Saves data back to files.

3. **Notifications.xaml.cs** — A class for displaying notifications and sorting events.
   - Allows sorting events by date.
   - Includes functions for managing event display and user interaction.

4. **Container.cs** — A class serving as a container for global data.
   - Contains static fields for storing the event list, end flags, and new notification flags.
---
## Installation

1. Clone the repository

2. Open the project in **Visual Studio**.

3. Build and run the project:
- Open **Solution Explorer**.
- Right-click the project and select **Build**.
- Run the application via **Start**.
---
## Main Methods and Functionalities

### AgrMachinery

- **isWorkingNowCheck()**: Checks if the machinery is working at the current moment.
- **CalculationOfHoursSpentAndFuel()**: Calculates the amount of fuel spent and work hours.
- **ConvertDataToTimeStr()**: Converts start and end times to string format for display.

### SaveAndLoadService

- **LoadAllAgrMachinery()**: Loads the list of all machinery from a file.
- **SaveAllAgrMachinery()**: Saves the list of machinery to a file.
- **LoadEventsList()**: Loads the list of events from a file.
- **SaveEventsList()**: Saves the list of events to a file.

### Notifications

- **SortBtn_Click()**: Sorts events by the selected date.
- **AllList_Click()**: Displays all events without filters.
- **BackToTableBtn_Click()**: Returns to the main event list.
- **ClearTabl_MouseDown()**: Clears the event table.
---
## Technical Requirements

- **.NET Framework 4.7.2** or higher.
- **Visual Studio** for building and running the project.
---
## Important Notes

- Upon first run, the program creates empty files to store data if they do not exist.
- All data is saved in JSON format, which makes it easy to export and import machinery and event information.

---

**Developed by Escapist_1871**
