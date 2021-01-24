select Name, count(Name) as Gp, round(Avg(Min)) as Min, sum(Pts) as Pts, sum(Reb) as Reb, sum(Ast) as Ast, sum(Tpm) as Tpm, sum(Stl) as Stl, sum(Blk) as Blk, sum("To") as 'To',
       sum(Fta) as Fta, sum(Fga) as Fga, ifnull(sum(Ftm)/sum(Fta), 0) as FtPer, ifnull(sum(Fgm)/sum(Fga),0) as FgPer
from Players ,Games
where ID==PlayerId and GameDate > Datetime('2020-10-01') and Min>0
group by Name