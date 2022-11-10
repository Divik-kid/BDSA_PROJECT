namespace GitInsight.Core;

public record ContributionDTO(string RepoPath, string Author, DateTime Date, int CommitsCount); //int Id, 

public record ContributionCreateDTO(string RepoPath, string Author, DateTime Date, int CommitsCount);

public record ContributionUpdateDTO(string RepoPath, string Author, DateTime Date, int NewCommitsCount); //int Id, 
