/*
 * Copyright (C) 2024 Vincent Fontaine
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */

namespace BookingKata.Infrastructure.Storage;

public partial class SalesRepository
{
    public IQueryable<Vacancy> Vacancies

        => _sales.Vacancies
            .Where(vacancy => !vacancy.Cancelled)
            .AsNoTracking();

    public void AddVacancies(IEnumerable<Vacancy> newVacancies)
    {
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

        _sales.SaveChanges();

        foreach (var vacancy in toBeAdded)
        {
            _sales.Entry(vacancy).State = EntityState.Detached;
        }
    }

    public void RemoveVacancies(IEnumerable<long> vacancyIds)
    {
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

        _sales.SaveChanges();
        
        foreach (var vacancy in toBeRemoved)
        {
            _sales.Entry(vacancy).State = EntityState.Detached;
        }
    }
}