dat = read.csv("../data/20160402-WarOnIce.csv", na.strings=c("NA", ""))

dat = dat[,-c(1,2,4,6)]
dat = dat[which(dat$Salary > 0),]
dat$SalaryLog = log(dat$Salary)
dat$posSimp = "F"
dat[grep("D", dat$pos), "posSimp"] = "D"
dat = dat[,-c(1)]


library(leaps)

# Any 0 salaries should be removed I suppose



dat
library(LiblineaR)

attach(dat)

dat
dat$position=ifelse(dat$posSimp=='f',1,0)
dat=subset.data.frame(dat, select=-c(posSimp,position))

s=scale(dat,center=TRUE,scale=TRUE)
na.omit(t(dat))
x=subset.data.frame(dat, select=-c(Salary))
y=subset.data.frame(dat, select=Salary)

na.omit(dat)
n=nrow(dat)
d=ncol(dat)
xTrain=x[1:4*n/5,]
xTest=x[4*n/5:n,]
yTrain=y[1:4*n/5,]
yTest=y[4*n/5:n,]
xTrain

# Find the best model with the best cost parameter via 10-fold cross-validations
tryTypes=c(11:13)
tryCosts=c(1000,1,0.001)
bestCost=NA
bestAcc=0
bestType=NA
y
xTrain
s
accd=LiblineaR(data=s,target=yTrain,svr_eps=.01,type=11,cost=2,bias=TRUE,verbose=FALSE)
y
for(co in tryCosts){
  accd=LiblineaR(data=s,target=yTrain,type=ty,svr_eps=.01,cost=co,bias=TRUE,verbose=FALSE)
}
accd
for(ty in tryTypes){
  for(co in tryCosts){
    accd=LiblineaR(data=s,target=yTrain,type=ty,cost=co,bias=TRUE,verbose=FALSE)
    accd
    if(accd>bestAcc){
      bestCost=co
      bestAcc=accd
      bestType=ty
    }
  }
}
bestType
bestCost
bestAcc





 mod = regsubsets(SalaryLog ~ posSimp+Gm+Age+G+A1+A2+G60+A60+P60+PenD+CF.+PDO+PSh.+ZSO.Rel+TOI.Gm, data=dat, method="exhaustive", nvmax = 20)
 smod = summary(mod)
 smod$adjr2
 which.max(smod$adjr2)
 
 
 