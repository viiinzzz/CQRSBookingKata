namespace CQRSBookingKata.API;

public partial class SalesRepository
{
    public IQueryable<Vacancy> Vacancies

        => _sales.Vacancies
            .Where(vacancy => !vacancy.Cancelled)
            .AsNoTracking();

    public void AddVacancies(IEnumerable<Vacancy> newVacancies, bool scoped)
    {
        try
        {
            using var scope = !scoped ? null : new TransactionScope();

            var toBeAdded =
                from newVacancy in newVacancies
                //left outer join
                join curVacancy in _sales.Vacancies
                    on newVacancy.VacancyId equals curVacancy.VacancyId into alreadyExist
                from already in alreadyExist.DefaultIfEmpty()
                where already is null
                select newVacancy;

            if (!toBeAdded.Any())
            {
                return;
            }

            _sales.Vacancies.AddRange(toBeAdded);

            foreach (var vacancy in toBeAdded)
            {
                _sales.Entry(vacancy).State = EntityState.Detached;
            }

            _sales.SaveChanges();

            scope?.Complete();
        }
        catch (Exception e)
        {
            throw new ServerErrorException(e);
        }
    }

    public void RemoveVacancies(IEnumerable<long> vacancyIds, bool scoped)
    {
        try
        {
            using var scope = !scoped ? null : new TransactionScope();

            var toBeRemoved = _sales.Vacancies
                // .AsNoTracking()
                .Where(match => vacancyIds
                    .Contains(match.VacancyId))
                .ToArray();

            if (!toBeRemoved.Any())
            {
                return;
            }

            _sales.Vacancies.RemoveRange(toBeRemoved);

            foreach (var vacancy in toBeRemoved)
            {
                _sales.Entry(vacancy).State = EntityState.Detached;
            }

            _sales.SaveChanges();

            scope?.Complete();
        }
        catch (Exception e)
        {
            throw new ServerErrorException(e);
        }
    }
}