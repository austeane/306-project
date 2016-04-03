dat = read.csv("../data/20160402-WarOnIce.csv", na.strings=c("NA", ""))
dat = dat[,-c(1,2,4,6)]
dat = dat[which(dat$Salary > 0),]
dat$SalaryLog = log(dat$Salary)
dat$AAVLog = log(dat$AAV)
dat$posSimp = "F"
dat[grep("D", dat$pos), "posSimp"] = "D"
dat = dat[,-c(1)]
dat = dat[,-(17:93)]
dat = dat[,-(19:35)]
dat = dat[which(dat$Gm >= 15),]
dat2 = dat[,c(3,19,1,2,5,8,17,18,9,10,11,12,13,14,15,16,20)]

names(dat)

library(leaps)

# Create plots of a few explanatory variable transformations
png(filename = "ExplanatoryPlots.png", width = 1200, height = 1200, units = "px", pointsize = 24, bg = "white")
par(mfrow=c(4,3), mar=c(4,4,1,1))
plot(dat$Age, dat$Salary, xlab="Age", ylab="Salary")
plot(dat$TOI.Gm, dat$Salary, xlab="Time on ice per game", ylab="Salary")
boxplot(Salary~posSimp, data=dat, xlab="Position", ylab="Salary")
plot(dat$G, dat$Salary, xlab="G", ylab="Salary")
plot(dat$A1, dat$Salary, xlab="A1", ylab="Salary")
plot(dat$A2, dat$Salary, xlab="A2", ylab="Salary")

plot(dat$Age, dat$SalaryLog, xlab="Age", ylab="log(Salary)")
plot(dat$TOI.Gm, dat$SalaryLog, xlab="Time on ice per game", ylab="log(Salary)")
boxplot(SalaryLog~posSimp, data=dat, xlab="Position", ylab="log(Salary)")
plot(dat$G, dat$SalaryLog, xlab="G", ylab="log(Salary)")
plot(dat$A1, dat$SalaryLog, xlab="A1", ylab="log(Salary)")
plot(dat$A2, dat$SalaryLog, xlab="A2", ylab="log(Salary)")
dev.off()

# Create plots of a few explanatory variable transformations
png(filename = "ExplanatoryPlots2.png", width = 1200, height = 900, units = "px", pointsize = 24, bg = "white")
par(mfrow=c(3,3), mar=c(4,4,1,1))
plot(dat$Gm, dat$SalaryLog, xlab="Games Played", ylab="log(Salary)")
plot(dat$G60, dat$SalaryLog, xlab="Goals / 60m", ylab="log(Salary)")
plot(dat$A60, dat$SalaryLog, xlab="Assists / 60m", ylab="log(Salary)")
plot(dat$PenD, dat$SalaryLog, xlab="Penalty Differential", ylab="log(Salary)")
plot(dat$CF., dat$SalaryLog, xlab="Corsi For %", ylab="log(Salary)")
plot(dat$PDO, dat$SalaryLog, xlab="PDO", ylab="log(Salary)")
plot(dat$PSh., dat$SalaryLog, xlab="Personal Shooting %", ylab="log(Salary)")
plot(dat$ZSO.Rel, dat$SalaryLog, xlab="Offensive Zone Starts", ylab="log(Salary)")
dev.off()

png(filename = "ResidualPlots.png", width = 1200, height = 800, units = "px", pointsize = 24, bg = "white")
par(mfrow=c(2,3), mar=c(4,4,1,1))
plot(dat$Age, resid(lm(SalaryLog~Age, data=dat)), ylab="Residual", xlab="Age")
abline(0,0, col="red")
plot(dat$G, resid(lm(SalaryLog~G, data=dat)), ylab="Residual", xlab="G")
abline(0,0, col="red")
plot(dat$A1, resid(lm(SalaryLog~A1, data=dat)), ylab="Residual", xlab="A1")
abline(0,0, col="red")
plot(dat$TOI.Gm, resid(lm(SalaryLog~TOI.Gm, data=dat)), ylab="Residual", xlab="Time on ice / game")
abline(0,0, col="red")
boxplot(resid(lm(SalaryLog~posSimp, data=dat)) ~ dat$posSimp, data = dat, ylab="Residual", xlab="Position")
dev.off()

 mod = regsubsets(SalaryLog ~ ., data=dat2[,-c(1)], method="exhaustive", nvmax = 20)
 smod = summary(mod)
 smod$adjr2
 which.max(smod$adjr2)
 smod
 
 coef(mod, 5)
 
 modA = regsubsets(AAVLog ~ Gm+Age+posSimp+G60+G+A1+A2+A60+P60+TOI.Gm+PSh.+PenD+CF.+PDO+ZSO.Rel, data=dat, method="exhaustive", nvmax = 20)
 smodA = summary(modA)
 smodA$adjr2
 which.max(smodA$adjr2)
 smodA 
 
 dat2

 mod1 = lm(SalaryLog ~ Gm+Age+posSimp+G60+A1+A2+P60+TOI.Gm+PSh., data=dat)
 smod1 = summary(mod1)
 salaries = data.frame(dat$Name, dat$Salary, exp(predict(mod1, newdata=dat, interval="prediction")))
 salaries
 smod1
 
 
 
 mod2 = lm(AAVLog ~ Age+G+A1+TOI.Gm+posSimp, data=dat)
 smod2 = summary(mod2)
 smod2
 salaries = data.frame(dat$Name, dat$Salary, dat$AAV, exp(predict(mod2, newdata=dat, interval="prediction")))
 salaries
 
 mean(exp(predict(mod1, newdata=dat2)))
 mean(dat$Salary)
 
 diff = dat$SalaryLog - predict(mod1, newdata=dat)
 sum(abs(diff))
 ?predict.lm

  
 mod2 = lm(SalaryLog ~ posSimp+Age+G60+A60+TOI.Gm, data=dat)
 smod2 = summary(mod2)
 smod2
 
 dat$Name
 
 names(dat)
 summary(dat[,-c(20)])
 

 ct = cor(dat2[,-c(17)])
 
 write.csv(ct, "corr.csv")
 