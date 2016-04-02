dat = read.csv("../data/20160402-WarOnIce.csv", na.strings=c("NA", ""))
dat = dat[,-c(1,2,4,6)]
dat = dat[which(dat$Salary > 0),]
dat$SalaryLog = log(dat$Salary)
dat$posSimp = "F"
dat[grep("D", dat$pos), "posSimp"] = "D"
dat = dat[,-c(1)]

library(leaps)


# Create plots of a few explanatory variable transformations
png(filename = "ExplanatoryPlots.png", width = 1200, height = 1200, units = "px", pointsize = 24, bg = "white")
par(mfrow=c(3,3), mar=c(4,4,1,1))
plot(dat$Age, dat$Salary, xlab="Age", ylab="Salary")
plot(dat$G, dat$Salary, xlab="G", ylab="Salary")
plot(dat$TOI.Gm, dat$Salary, xlab="Time on ice per game", ylab="Salary")
plot(dat$Age, dat$SalaryLog, xlab="Age", ylab="log(Salary)")
plot(dat$G, dat$SalaryLog, xlab="G", ylab="log(Salary)")
plot(dat$TOI.Gm, dat$SalaryLog, xlab="Time on ice per game", ylab="log(Salary)")
plot(dat$A1, dat$SalaryLog, xlab="A1", ylab="log(Salary)")
plot(dat$A2, dat$SalaryLog, xlab="A2", ylab="log(Salary)")
dev.off()

png(filename = "ResidualPlots.png", width = 1200, height = 1200, units = "px", pointsize = 24, bg = "white")
par(mfrow=c(3,3), mar=c(4,4,1,1))
plot(dat$Age, resid(lm(SalaryLog~Age, data=dat)), ylab="Residual", xlab="Age")
abline(0,0, col="red")
plot(dat$TOI.Gm, resid(lm(SalaryLog~TOI.Gm, data=dat)), ylab="Residual", xlab="Time on ice per game")
abline(0,0, col="red")
plot(dat$G, resid(lm(SalaryLog~G, data=dat)), ylab="Residual", xlab="G")
abline(0,0, col="red")
plot(dat$A1, resid(lm(SalaryLog~A1, data=dat)), ylab="Residual", xlab="A1")
abline(0,0, col="red")
plot(dat$A2, resid(lm(SalaryLog~A2, data=dat)), ylab="Residual", xlab="A2")
abline(0,0, col="red")
dev.off()

 mod = regsubsets(SalaryLog ~ posSimp+Gm+Age+G+A1+A2+G60+A60+PenD+CF.+PDO+PSh.+ZSO.Rel+TOI.Gm+FF., data=dat, method="exhaustive", nvmax = 20)
 smod = summary(mod)
 smod$adjr2
 which.max(smod$adjr2)
 smod
 
 mod2 = lm(SalaryLog ~ Age+G+A1+A2+TOI.Gm, data=dat)
 smod2 = summary(mod2)
 smod2
 