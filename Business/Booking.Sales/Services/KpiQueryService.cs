﻿/*
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

namespace BookingKata.Sales;

public class KpiQueryService
(
    ISalesRepository sales

    // IAdminRepository admin,
    // IMoneyRepository money,
    // IPlanningRepository planning,
    // ITimeService DateTime
)
{
  
    public double GetOccupancyRate(int hotelId)
    {
       var vacancyCount = sales.Vacancies
           .Where(v => v.UniqueRoomId / 1_0000 == hotelId)
           .Count(v => !v.Cancelled);

       var bookCount = sales.Bookings
           .Where(b => b.UniqueRoomId / 1_0000 == hotelId)
           .Count(b => !b.Cancelled);

       return 1.0 * bookCount / (vacancyCount + bookCount);
    }



}


/*

Average Daily Rate (ADR)
Average daily rate, or ADR, measures the average revenue generated by rooms sold in a given time period.
Complimentary or vacant rooms should not be included in this metric. ADR is also factored into revenue per available room (RevPAR). 
However, where RevPAR is based on all available rooms (supply), ADR is based only on rooms sold (demand). 
The reason hotels calculate ADR is to get an idea of the maximum different types of guests are willing to spend and is an important part of benchmarking.
It will help find out if your ADR is impacting your occupancy levels, for example.
=Average Revenue Earned / Number of Rooms Sold

Revenue Per Available Room (RevPAR)
Revenue per available room, or RevPAR, uses both ADR and occupancy rate to measure performance in any given period as well as potential gains or losses in revenue.
This percentage can be compared to your competitive set, allowing you to see how you stack up against the competition.
RevPAR can help you assess performance, set rates, and better evaluate your marketing campaigns and promotions.
=Rooms Revenue / Rooms Available or Average Daily Rate x Occupancy Rate

Gross Operating Profit (GOP)
Gross operating profit, or GOP, refers to a hotel’s profit after subtracting operating expenses across departments.
This is how you calculate the operational profitability of a hotel. This calculation does not take into account non-operating expenses such as tax, however. 
This metric helps you determine whether your property is spending more revenue than it is earning or vice versa. 
=Gross Operating Revenue – Gross Operating Expenses

Gross Operating Profit per Available Room (GOPPAR)
Gross operating profit per available room, or GOPPAR, measures the relationship between a property’s revenue and expenses from a rooms-available basis.
Unlike RevPAR, this metric provides a much clearer picture of a hotel’s profitability as it takes into account revenue and operating costs across departments. 
GOPPAR helps hotels determine when and if their expenses outweigh revenue, and if there are any times of year where this occurs, such as off-season periods. 
=Gross operating profit (GOP) / Total number of available room nights

Market Penetration Index (MPI)
Your market penetration index (MPI) — also sometimes called an occupancy index — determines your market share of occupancy compared to competitors. 
An MPI greater than 100 indicates that your hotel has a larger share than expected compared to the comp set. If it is lower than 100, you have less than the expected share.
For example, if your occupancy rate is 70% and the occupancy of your comp set is 70%, then your MPI would be 100.
Your MPI helps you understand how your occupancy rate stacks up to competitors in your local market so you can analyze and adjust your pricing. 
=(Your occupancy rate / Aggregated group of hotels’ occupancy rate) x 100

Average Length of Stay (ALOS)
Your average length of stay, or ALOS, is the average number of nights guests stay at your hotel.
This is an important metric to consider as shorter stays are usually less profitable due to operating costs.
Hotels should look for ways to increase the average length of stay in order to reduce the costs per room occupied.   
=Total Occupied Room Nights / Total Number of Bookings

Revenue Generation Index (RGI)
Your revenue generation index, or RGI, measures your hotel’s performance and occupancy rate against that of your comp set. 
This metric will help you determine your share of market revenue compared to competitors. 
=Your RevPAR / Your Competitors’ RevPAR 

Average Room Rate (ARR)
While your average daily rate calculates your rates per day, your average room rate, or ARR, allows you to calculate them over a specified period of time.
This metric allows you to measure the success of particular months or seasons, and compare that with your comp set. 
=Total room revenue / Total rooms occupied

Total Revenue per Available Room (TrevPAR)
Total Revenue per Available Room, or TrevPAR, gives a more holistic view of your overall hotel revenue. Unlike RevPAR, it calculates the total revenue generated per room, including amenities and add-ons etc.,
rather than room bookings alone. Note, however, that TrevPAR does not take expenses into account, meaning it cannot be used alone. 
=Total Revenue / Rooms Available


Earnings Before Interest, Taxes, Depreciation, & Amortization (EBITDA)
A hotel’s earning before interest, taxes, depreciation, and amortization, or EBITDA, evaluates your hotel’s net income with interest, taxes, depreciation and amortization added back.
This is a great metric to help you compare the financial performance of multiple properties in different countries, for example.
=Net Income + Taxes + Interest Expense + Depreciation & Amortization or Operating Income + Depreciation & Amortization

Net Revenue Per Available Room (NRevPAR) 
Net revenue per available room, or NRevPAR, helps you understand how much revenue you generate from available rooms with costs associated with commissions, transaction fees, and loyalty expenses subtracted.
This metric is typically used alongside other hotel revenue management metrics in order to adjust pricing. 
=NRevPAR: (Room Revenue – Distribution Costs) / Number of Available Rooms

Revenue Per Occupied Room (RevPOR)
Revenue per occupied room, or RevPOR calculates how much revenue your rooms generate without considering seasonality.
It considers upsells such as dry cleaning, mini bar, or spa treatments. 
=Total Revenue / Number of Occupied Rooms

Cost per Occupied Room (CPOR)
Cost per occupied room, or CPOR, helps hotels to assess hotel expenses and work out a minimum room rate to ensure enough is being charged to cover costs and create a profit margin. 
= Gross Operating Expenses / Total Number of Rooms Sold

Revenue per Available Seating Hour (RevPASH)
Your revenue per available seating hour, or RevPASH, is a great metric to use if you have restaurants or bars in your property. 
It allows you to calculate the usage and revenue of a seat per hour, which allows you to better schedule labor, food ordering, and marketing during times of low occupancy.
RevPASH = ‍Total Outlet Revenue / (Available Seats x Opening Hours)
 */