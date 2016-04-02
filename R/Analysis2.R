dat = read.csv("../data/20160402-WarOnIce.csv", na.strings=c("NA", ""))
dat = dat[,-c(1,2,4,6)]
dat = dat[which(dat$Salary > 0),]
dat$SalaryLog = log(dat$Salary)
dat$posSimp = "F"
dat[grep("D", dat$pos), "posSimp"] = "D"
dat = dat[,-c(1)]

library(leaps)

  

 mod = regsubsets(SalaryLog ~ posSimp+Gm+Age+G+A1+A2+G60+A60+P60+PenD+CF.+PDO+PSh.+ZSO.Rel+TOI.Gm, data=dat, method="exhaustive", nvmax = 20)
 smod = summary(mod)
 smod$adjr2
 which.max(smod$adjr2)
 
 
 