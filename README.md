﻿# Ad Budget Goal Seek Optimizer (.NET Core) 

![Demo](./ReadmeAssets/demo.png)

This optimizer calculates the optimized budget for a specific ad within a campaign. To calculate the result, the users need to provide following parameters:

| Name | Description |
|----------|----------|
| Total campaign budget | Total budget for the campaign. |
| Agency fee percentage | Agency fees, calculated as a percentage of the total ad spend. |
| Third-party tool fee percentage | Fees for ads created using third-party tools, calculated as a percentage of the total third party ad spend. |
| Fixed costs agencty hours | Fixed costs for agency hours. |
| Other ad budgets | Budgets for other ads. |

The optimizer also provides availability to adjust following settings:

| Name | Description |
|----------|----------|
| Initial guess | Initial guess before the calculation. |
| Is with a third party tool? | Whether this ad uses a third party tool. |
| Tolerance | Tolerance for the goal seek function. |
| Max iteration | The max iteration for the goal seek function. |

The goal-seek function is implemented with the Newton-Raphson method.
https://www.reddit.com/r/excel/comments/81coc9/how_to_solve_for_a_goal_seek_without_using_goal/

## How to run this project
The project is driven by .NET Core with .NETFramework 8.0. To run this project, please install [.NET 8.0 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/8.0) first.

To run the project, run the following command with Powershell in `BudgetOptimizerApp` folder:
```
dotnet watch
```

To run the unit test, run the following command with Powershell in `BudgetOptimizerApp.Test` folder:
```
dotnet test
```

## Version Control

This project follows [GitFlow](http://datasift.github.io/gitflow/IntroducingGitFlow.html) version control rules, but not that strict.

Hereby a brief guide:

- For the `main` branch, you cannot push commits into it directly. It can only be merged from any `release` branch with a pull request. We need to make sure that every version of the `main` branch is the latest production-ready game.
- For the `develop` branch, you cannot push commits into it directly. It can only be merged from any `feature/xxx_xxx` branch with a pull request. We need to make sure that every version of the `develop` branch is a runnable game.
- If you want to add a new feature or maybe refactor any code structure, **you should create a branch named `feature/the_name_of_feature` from the develop branch.** When the branch is finished, you can create a pull request for updating the `develop` branch.
- When a `develop` branch is ready to be published, you can create a pull request to create a `release/vx.x.x` branch. The name of a branch could be `release/v0.0.1` or `release/alpha1`.
