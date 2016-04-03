###Lab 8 Multiple Linear Regression
###Cross-validation: Leave-one-out, two-fold, five-fold.

#Download the "richmondcondo" data set from the course website.
#The description of it can be found in Lab 6

#Read in the data set#Or dat <- read.table("./Downloads/richmondcondo.txt", header=T, skip=2)

#Have a look at the data

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
# set up folds for cross-validation
folds <- cvFolds(nrow(dat), K = 5, R = 10)
## compare LS, MM and LTS regression
# perform cross-validation for an LS regression model
fitLm <- lm(SalaryLog ~ ., data = dat)
cvFitLm <- cvLm(fitLm, cost = rtmspe,
                folds = folds, trim = 0.1)

# perform cross-validation for an MM regression model
fitLmrob <- lmrob(SalaryLog ~ ., data = dat)
cvFitLmrob <- cvLmrob(fitLmrob, cost = rtmspe,
                      folds = folds, trim = 0.1)
# perform cross-validation for an LTS regression model
fitLts <- ltsReg(SalaryLog ~ ., data = dat)
cvFitLts <- cvLts(fitLts, cost = rtmspe,
                  folds = folds, trim = 0.1)
# compare cross-validation results
cvSelect(LS = cvFitLm, MM = cvFitLmrob, LTS = cvFitLts)
## compare raw and reweighted LTS estimators for
## 50% and 75% subsets
# 50% subsets
fitLts50 <- ltsReg(SalaryLog ~ ., data = dat, alpha = 0.5)
cvFitLts50 <- cvLts(fitLts50, cost = rtmspe, folds = folds,
                    fit = "both", trim = 0.1)
# 75% subsets

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




######################################
##Part 1: Leave-one-out Cross validaion.
######################################

#Function to calculate the leave-one-out cross validation error.
ls.cvrmse <- function(ls.out)
  # Compute the leave-one-out cross-validated root mean squared error of prediction.
  # Handles missing values.
  # ls.out is a fitted regression model from lsreg or lm.
  # (c) Copyright William J. Welch 1997
{
  res.cv <- ls.out$residuals / (1.0 - ls.diag(ls.out)$hat)
  # Identify NA's and remove them.
  is.na.res <- is.na(res.cv)
  res.cv <- res.cv[!is.na.res]
  cvrmse <- sqrt(sum(res.cv^2) / length(res.cv))
  return(cvrmse)
}


#Compare the full model and best model found by regsubsets
fullModel <- lm(SalaryLog~., data=dat)
summary(fullModel)

bestModel <- lm(askprice~ffarea+beds+floor+view+age+mfee, data=dat)
#Recall that this is the best model based on Mallow's cp and adjusted R^2
summary(bestModel)

#Calculate the leave-one-out CV RMSE for the full model
fullModel.cvrmse <- ls.cvrmse(fullModel)

#Calculate the leave-one-out CV RMSE for the best model via regsubsets
bestModel.cvrmse <- ls.cvrmse(bestModel)

print(c(fullModel.cvrmse, bestModel.cvrmse))
#The best model has smaller cvrmse

#########################################
###Part 2.2 Two-fold CV
########################################
n <- nrow(dat)
set.seed(1)
# here select half of the data randomly. 
id.subset1 <- sort(sample(1:n, round(n/2), replace = FALSE))

# Initially subset1 is the training set and subset2 is the hold-out set 
dat.subset1 <- dat[id.subset1,]
dat.subset2 <- dat[-id.subset1,] 

fullModel.subset1 <- lm(askprice~., data = dat.subset1)

# Make predictions at the hold-out data set.
fullModel.pred1 <- predict(fullModel.subset1, dat.subset2)
fullModel.err1 <- sqrt(sum((dat.subset2$askprice - fullModel.pred1)^2)/length(fullModel.pred1))
fullModel.err1

# Compare to the selected model.
bestModel.subset1 <- lm(askprice~ffarea+beds+floor+view+age+mfee, data = dat.subset1)
bestModel.pred1 <- predict(bestModel.subset1, dat.subset2)
bestModel.err1 <- sqrt(sum((dat.subset2$askprice - bestModel.pred1)^2)/length(bestModel.pred1))
bestModel.err1
# Smaller than that from the full model.

## Reverse training and hold-out sets (subset2 is training, subset1 is holdout)

fullModel.subset2 <- lm(askprice~., data = dat.subset2)

# Make predictions at the hold-out data set.
fullModel.pred2 <- predict(fullModel.subset2, dat.subset1)
fullModel.err2 <- sqrt(sum((dat.subset1$askprice - fullModel.pred2)^2)/length(fullModel.pred2))
fullModel.err2

## average prediction error for the full model ##
(fullModel.err1  + fullModel.err2)/2

# For the selected model
bestModel.subset2 <- lm(askprice~ffarea+ beds+ view+age+mfee, data = dat.subset2)
bestModel.pred2 <- predict(bestModel.subset2, dat.subset1)
bestModel.err2 <- sqrt(sum((dat.subset1$askprice - bestModel.pred2)^2)/length(bestModel.pred2))
bestModel.err2

## average prediction error ##
(bestModel.err1  + bestModel.err2)/2
## Best model based on regsubsets has smaller two-fold CVRMSE


###################################################
####Part 3 5-fold cross validation.
###################################################

n <- nrow(dat)
sn <- floor(n/5)
# 17, doesn't matter if you use round, floor, ceiling. 

set.seed(306)
B <- 500 #Do 500 random splits
errMx <- matrix(NA, B, 2) #matrix to store the results
colnames(errMx) <- c("FullModel", "BestModel")
for (i in 1:B)
{
  testInd <- sample(1:n, sn, replace=FALSE)
  
  tTestDat <- dat[testInd, ] #Treat the sampled index as testing set
  tTrainDat <- dat[-testInd, ] #The rest is training set.
  
  tFullModel <- lm(askprice~., data = tTrainDat)
  tFullModel.pred <- predict(tFullModel, tTestDat)
  errMx[i, 1] <- sqrt(sum((tTestDat$askprice - tFullModel.pred)^2)/sn)
  
  
  tBestModel <- lm(askprice~ffarea+ beds+ view+age+mfee, data = tTrainDat)
  tBestModel.pred <- predict(tBestModel, tTestDat)
  errMx[i, 2] <- sqrt(sum((tTestDat$askprice - tBestModel.pred)^2)/sn)
}

apply(errMx, 2, mean)
# FullModel BestModel 
# 10.82297  10.79276