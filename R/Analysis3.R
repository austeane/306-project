dat = read.csv("../data/20160402-WarOnIce.csv", na.strings=c("NA", ""))
library(cvTools)
dat = dat[,-c(1,2,4,6)]
dat = dat[which(dat$Salary > 0),]
dat$SalaryLog = log(dat$Salary)
dat$posSimp="F"
dat[grep("D", dat$pos), "posSimp"] = "D"
dat = dat[,-c(1)]



library(leaps)

# Any 0 salaries should be removed I suppose



dat
library(LiblineaR)

attach(dat)

dat$posSimp=ifelse(dat$posSimp=="F",1,0)
dat$posSimp
dat=subset.data.frame(dat, select=c(SalaryLog,posSimp,Gm,Age,G,A1,A2,G60,A60,PenD,CF.,PDO,PSh.,ZSO.Rel,TOI.Gm))
dat=subset.data.frame(dat, select=-c(posSimp,position,AAV,Salary))

s=scale(dat,center=TRUE,scale=TRUE)
na.omit(t(dat))
x=subset.data.frame(dat, select=-c(Salary))
y=subset.data.frame(dat, select=Salary)

na.omit(dat)
n=nrow(y)

d=ncol(dat)
xTrain=as.data.frame(x[1:600,])
xTest=as.data.frame(x[601:840,])
yTrain=as.data.frame(y[1:600,1])
yTest=as.data.frame(y[601:840,1])
yTrain=as.data.frame(yTrain)

dat$posSimp=ifelse(dat$posSimp,1)
dat
set.seed(1234) # set seed for reproducibility


fit=rep(0,15)

fit1=cvLm(lm(SalaryLog ~ Age, data = dat),cost=rtmspe,folds=folds,trim=.1)
fit2=cvLm(lm(SalaryLog ~ Age+TOI.Gm, data = dat),cost=rtmspe,folds=folds,trim=.1)
fit3=cvLm(lm(SalaryLog ~ Age+TOI.Gm+posSimp, data = dat),cost=rtmspe,folds=folds,trim=.1)
fit4=cvLm(lm(SalaryLog ~ A1+Age+TOI.Gm+posSimp, data = dat),cost=rtmspe,folds=folds,trim=.1)
fit5=cvLm(lm(SalaryLog ~ G+A1+Age+TOI.Gm+posSimp, data = dat),cost=rtmspe,folds=folds,trim=.1)
fit6=cvLm(lm(SalaryLog ~ PSh.+G+A1+Age+TOI.Gm+posSimp, data = dat),cost=rtmspe,folds=folds,trim=.1)
fit7=cvLm(lm(SalaryLog ~ A1+PSh.+G+A1+Age+TOI.Gm+posSimp, data = dat),cost=rtmspe,folds=folds,trim=.1)
fit8=cvLm(lm(SalaryLog ~ ZSO.Rel+A1+PSh.+G+A1+Age+TOI.Gm+posSimp, data = dat),cost=rtmspe,folds=folds,trim=.1)
fit9=cvLm(lm(SalaryLog ~ Gm+ZSO.Rel+A1+PSh.+G+A1+Age+TOI.Gm+posSimp, data = dat),cost=rtmspe,folds=folds,trim=.1)
fit10=cvLm(lm(SalaryLog ~ A60+Gm+ZSO.Rel+A1+PSh.+G+A1+Age+TOI.Gm+posSimp, data = dat),cost=rtmspe,folds=folds,trim=.1)
fit11=cvLm(lm(SalaryLog ~ PDO+A60+Gm+ZSO.Rel+A1+PSh.+G+A1+Age+TOI.Gm+posSimp, data = dat),cost=rtmspe,folds=folds,trim=.1)
fit12=cvLm(lm(SalaryLog ~ CF.+PDO+A60+Gm+ZSO.Rel+A1+PSh.+G+A1+Age+TOI.Gm+posSimp, data = dat),cost=rtmspe,folds=folds,trim=.1)
fit13=cvLm(lm(SalaryLog ~ G60+CF.+PDO+A60+Gm+ZSO.Rel+A1+PSh.+G+A1+Age+TOI.Gm+posSimp, data = dat),cost=rtmspe,folds=folds,trim=.1)
fit14=cvLm(lm(SalaryLog ~ P60+G60+CF.+PDO+A60+Gm+ZSO.Rel+A1+PSh.+G+A1+Age+TOI.Gm+posSimp, data = dat),cost=rtmspe,folds=folds,trim=.1)
fit15=cvLm(lm(SalaryLog ~ PenD+P60+G60+CF.+PDO+A60+Gm+ZSO.Rel+A1+PSh.+G+A1+Age+TOI.Gm+posSimp, data = dat),cost=rtmspe,folds=folds,trim=.1)

cvSelect(fit1,fit2,fit3,fit4,fit5,fit6,fit7,fit8,fit9,fit10,fit11,fit12,fit13,fit14,fit15)

max(fit)