namespace GitInsight.Entities;
using GitInsight.Core;
using LibGit2Sharp;

public class RepoCheckRepository {

    private GitInsightContext _context;

    public RepoCheckRepository(GitInsightContext context)
    {
        _context = context;
    }

    public static Contribution ContributionFromContributionDTO(ContributionDTO contribution)
        => new Contribution
         {
             author = contribution.Author,
             date = contribution.Date,
             commitsCount = contribution.CommitsCount
         };

    
    public void CreateEntryInDB(string folderPath){

        var repo = new Repository(folderPath);

        var checkedCommit = repo.Commits.ToList().First().Id.ToString();

        var conDTOs = AddContributionsDataToSet(repo);

        var newRepoCheck = new RepoCheck
                    {
                        repoPath = folderPath,
                        lastCheckedCommit = checkedCommit,
                        Contributions = conDTOs.Select(c => 
                        ContributionFromContributionDTO(c)).ToHashSet()
                    };

        _context.RepoChecks.Add(newRepoCheck);
        _context.SaveChanges();
    }

//-----------------------Helper methods-----------------
    public HashSet<ContributionDTO> AddContributionsDataToSet(Repository repo){
        //add repo data to hashset
        var commitArray = repo.Commits.ToList();
        var contributionsList = new HashSet<ContributionDTO>();

        foreach (var c in commitArray){
            //get number of commits by auhtor on date
            int commitNr = getNrCommitsOnDateByAuthor(c.Author.When.Date, c.Author, repo);

            var newContri = new ContributionDTO(
                Author: c.Author.ToString(), 
                Date: c.Author.When.Date,
                CommitsCount: commitNr);

            contributionsList.Add(newContri);
        }

        return contributionsList;
    }

    private int getNrCommitsOnDateByAuthor(DateTime date, Signature author, Repository repo){
        var commitsCount = repo.Commits
        .Select(e => new { e.Author, e.Author.When.Date })
        .Where(e => e.Author.ToString() == author.ToString()
        && e.Author.When.Date == date).Count();

        return commitsCount;
    }

//-------------------------------------------------

    
    public HashSet<ContributionDTO> ContributionToContributionDTOHS(RepoCheck repoCheck){
        var contributions = repoCheck.Contributions
                    .Select(cont => new ContributionDTO(
                        cont.author,
                        cont.date, cont.commitsCount
                    )).ToHashSet();
        return contributions;
    }

    //only use this in testing for now
    public RepoCheckDTO Read(string folderPath){
        var repoCheck = _context.RepoChecks.Find(folderPath);
        //var DTO = new RepoCheckDTO();
        return repoCheck != null ? new RepoCheckDTO(
                                    repoCheck.repoPath,
                                    repoCheck.lastCheckedCommit,
                                    Contributions: ContributionToContributionDTOHS(repoCheck)) : null!;
    }

    public List<comFreqObj> getCommitFreq(string folderPath){
        _context.Database.OpenConnection();

            var repoCheckItem = _context.RepoChecks.Find(folderPath); //check om commit newest, fix
            var items = _context.Contributions.Where(c => c.repoCheckObj.Equals(repoCheckItem));

            var date = items.Select(c => c.date.Date).Distinct().ToList();

            var intList = new List<int>();
            foreach(var d in date){
                var comCount = items.Where(k => k.date.Date.Equals(d))
                .Select(k => k.commitsCount).Sum();
                intList.Add(comCount);
            }

            var tempList = new List<comFreqObj>();
            for (var i = 0; i < intList.Count; i++){
                var tem = new comFreqObj(date[i].Date.ToString(), intList[i]);
                tempList.Add(tem);
            }

            return tempList;


    }

    public List<userComFreqObj> getUserCommitFreq(string folderPath){
        _context.Database.OpenConnection();

        var repoCheckItem = _context.RepoChecks.Find(folderPath); //check om commit newest, fix
        var contributions = _context.Contributions.Where(c => c.repoCheckObj.Equals(repoCheckItem));

        var authors = contributions.Select(c => c.author).Distinct().ToList();
        var data = new List<userComFreqObj>();
        foreach(string auth in authors){
            var intList = new List<int>();
            var contrList = new List<Tuple<string, int>>();

            var dates = contributions.Where(k => k.author.Equals(auth))
                        .Select(c => c.date).Distinct().ToList();

            foreach(var d in dates){
                var comCount = contributions.Where(k => k.date.Equals(d)
                && k.author.Equals(auth))
                .Select(k => k.commitsCount).Sum();
                intList.Add(comCount);
            }
            
            for (var i = 0; i < intList.Count; i++){
                var tempTuple = Tuple.Create(dates[i].Date.ToString(), intList[i]);
                contrList.Add(tempTuple);
            }

            data.Add(new userComFreqObj(auth, contrList));
        }
            
        return data;
        
    }

    public record comFreqObj(string date, int commits);

    public record userComFreqObj(string author, List<Tuple<string, int>> datesCommits);

    public bool CurrentCommitIdMostRecentCommit(string folderPath){
        var repoCheckObj = _context.RepoChecks.Find(folderPath);
        var repo = new Repository(folderPath);

        var NewestCommitId = repo.Commits.Last().Id;
        var curStoredCommitId = repoCheckObj.lastCheckedCommit;

        if(NewestCommitId.Equals(curStoredCommitId)){
            return true;
        } else return false;

    }

    public void Update(string folderPath){
        var toUpdate = _context.RepoChecks.Find(folderPath);
        var repo = new Repository(folderPath);
        var newestCommitId = repo.Commits.ToList().First().Id.ToString();

        var conDTOs = AddContributionsDataToSet(repo);

        var newCons = conDTOs.Select(c => 
        ContributionFromContributionDTO(c)).ToList();

        toUpdate.lastCheckedCommit = newestCommitId;
        toUpdate.Contributions = newCons; 


        _context.SaveChanges();
    }

}