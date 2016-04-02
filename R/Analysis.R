#First load the data set all as one
dat = read.csv("../data/20160401-Complete.csv");

# Any 0 salaries should be removed I suppose
dat = dat[-which(dat$Salary == 0),]
nrow(dat)
  
#Next, let's take a look at our response variable: Salary
par(mfrow=c(1,3), mar=c(2, 2, 2, 2))
hist(dat$Salary)
#Square root might give a better distribution of wages
hist(sqrt(dat$Salary))
#Log might also give a better distribution
hist(log(dat$Salary))
dat$SalaryRoot = sqrt(dat$Salary)
dat$SalaryLog = log(dat$Salary)
dat$G60 = dat$G * 60 / dat$TOI

# Make separate data groups for forwards vs. defense
datF = dat[which(dat[3]=="F"),]
nrow(datF)
datD = dat[which(dat[3]=="D"),]
nrow(datD)

#Create a linear model using all of the simple data
ModF = lm(Salary ~ G+A1+A2+iHSC+iSC+iCF+C...+F...+G...+CF+FF+ZSO+ZSD+AB+FO_W+FO_L+HIT+HIT.+PN+PN.+TOI+Age+ShotAttempts+ScoringChances+ShotsOnGoal+A3OnDirectGoals, data=datF)
sModF = summary(ModF)
sModF

ModFSQ = lm(SalaryRoot ~ G+I(G^2)+A1+A2+iHSC+iSC+iCF+C...+F...+G...+CF+FF+ZSO+ZSD+AB+FO_W+FO_L+HIT+HIT.+PN+PN.+TOI+Age+ShotAttempts+ScoringChances+ShotsOnGoal+A3OnDirectGoals, data=datF)
sModFSQ = summary(ModFSQ)
sModFSQ

ModFL = lm(SalaryLog ~ G+A1+A2+iHSC+iSC+iCF+C...+F...+G...+CF+FF+ZSO+ZSD+AB+FO_W+FO_L+HIT+HIT.+PN+PN.+TOI+Age+ShotAttempts+ScoringChances+ShotsOnGoal+A3OnDirectGoals, data=datF)
sModFL = summary(ModFL)
sModFL

# Plot some residuals off salary
par(mfrow=c(3,3), mar=c(4,4,1,1))
plot(datF$G, resid(lm(Salary~G, data=datF)), ylab="Residuals", xlab="Goals")
abline(0,0)
plot(datF$G^2, resid(lm(Salary~I(G^2), data=datF)), ylab="Residuals", xlab="Goals^2")
abline(0,0)
plot(datF$G60, resid(lm(SalaryLog~G60, data=datF)), ylab="Residuals", xlab="Goals Per Hour", xlim=c(0,2))
abline(0,0)
plot(datF$G60^2, resid(lm(SalaryLog~I(G60^2), data=datF)), ylab="Residuals", xlab="Goals Per Hour^2", xlim=c(0,5))
abline(0,0)
plot(datF$A1, resid(lm(Salary~A1, data=datF)), ylab="Residuals", xlab="First Assists")
abline(0,0)
plot(datF$A1^2, resid(lm(Salary~I(A1^2), data=datF)), ylab="Residuals", xlab="First Assists^2", xlim=c(0,50))
abline(0,0)
plot(datF$A2, resid(lm(Salary~A2, data=datF)), ylab="Residuals", xlab="Second Assists")
abline(0,0)
plot(datF$Age, resid(lm(SalaryLog~Age, data=datF)), ylab="Residuals", xlab="Age")
abline(0,0)
plot(datF$TOI, resid(lm(SalaryLog~TOI, data=datF)), ylab="Residuals", xlab="Time on ice", xlim=c(0, 400))
abline(0,0)

summary(lm(SalaryLog~G60+TOI+Age, data=datF))

library(leaps)
ModFSS = regsubsets(SalaryLog ~ G+G60+A1+A2+iHSC+iSC+iCF+C...+F...+G...+CF+FF+ZSO+ZSD+AB+FO_W+FO_L+HIT+HIT.+PN+PN.+TOI+Age+ShotAttempts+ScoringChances+ShotsOnGoal+A3OnDirectGoals, data=datF, method="exhaustive", nvmax=20)
sModFSS = summary(ModFSS)
sModFSS$adjr2
which.max(sModFSS$adjr2)
sModFSS

names(sModFSS)
names(sModFSS$obj)

