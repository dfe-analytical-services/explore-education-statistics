-- src/ holds two solutions. The Search one is the smaller of the two and does
-- not contain Data.Model, so letting Roslyn pick for itself loses cross-file
-- navigation across most of the repo.
vim.g.roslyn_solution = 'src/GovUk.Education.ExploreEducationStatistics.sln'
