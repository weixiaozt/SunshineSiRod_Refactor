-- MySQL dump 10.13  Distrib 8.0.33, for Win64 (x86_64)
--
-- Host: localhost    Database: siliconstick
-- ------------------------------------------------------
-- Server version	8.0.33

/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET @OLD_CHARACTER_SET_RESULTS=@@CHARACTER_SET_RESULTS */;
/*!40101 SET @OLD_COLLATION_CONNECTION=@@COLLATION_CONNECTION */;
/*!50503 SET NAMES utf8 */;
/*!40103 SET @OLD_TIME_ZONE=@@TIME_ZONE */;
/*!40103 SET TIME_ZONE='+00:00' */;
/*!40014 SET @OLD_UNIQUE_CHECKS=@@UNIQUE_CHECKS, UNIQUE_CHECKS=0 */;
/*!40014 SET @OLD_FOREIGN_KEY_CHECKS=@@FOREIGN_KEY_CHECKS, FOREIGN_KEY_CHECKS=0 */;
/*!40101 SET @OLD_SQL_MODE=@@SQL_MODE, SQL_MODE='NO_AUTO_VALUE_ON_ZERO' */;
/*!40111 SET @OLD_SQL_NOTES=@@SQL_NOTES, SQL_NOTES=0 */;

--
-- Table structure for table `stickresult`
--

DROP TABLE IF EXISTS `stickresult`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `stickresult` (
  `id` int NOT NULL AUTO_INCREMENT,
  `result` int DEFAULT NULL,
  `LJFile_1` varchar(255) CHARACTER SET gb2312 DEFAULT NULL,
  `LJFile_2` varchar(255) CHARACTER SET gb2312 DEFAULT NULL,
  `LJFile_3` varchar(255) CHARACTER SET gb2312 DEFAULT NULL,
  `LJFile_4` varchar(255) CHARACTER SET gb2312 DEFAULT NULL,
  `YinLieFile_1` varchar(255) CHARACTER SET gb2312 DEFAULT NULL,
  `YinLieFile_2` varchar(255) CHARACTER SET gb2312 DEFAULT NULL,
  `YinLieFile_3` varchar(255) CHARACTER SET gb2312 DEFAULT NULL,
  `YinLieFile_4` varchar(255) CHARACTER SET gb2312 DEFAULT NULL,
  `YingLiFile_1` varchar(255) CHARACTER SET gb2312 DEFAULT NULL,
  `YingLiFile_2` varchar(255) CHARACTER SET gb2312 DEFAULT NULL,
  `YingLiFile_3` varchar(255) CHARACTER SET gb2312 DEFAULT NULL,
  `YingLiFile_4` varchar(255) CHARACTER SET gb2312 DEFAULT NULL,
  `NGSubLJLength_1` varchar(255) COLLATE gb2312_bin DEFAULT NULL,
  `NGSubLJLength_2` varchar(255) COLLATE gb2312_bin DEFAULT NULL,
  `NGSubLJLength_3` varchar(255) COLLATE gb2312_bin DEFAULT NULL,
  `NGSubLJLength_4` varchar(255) COLLATE gb2312_bin DEFAULT NULL,
  `NGSubYinLieLength_1` varchar(255) COLLATE gb2312_bin DEFAULT NULL,
  `NGSubYinLieLength_2` varchar(255) COLLATE gb2312_bin DEFAULT NULL,
  `NGSubYinLieLength_3` varchar(255) COLLATE gb2312_bin DEFAULT NULL,
  `NGSubYinLieLength_4` varchar(255) COLLATE gb2312_bin DEFAULT NULL,
  `NGSubYingLiLength_1` varchar(255) COLLATE gb2312_bin DEFAULT NULL,
  `NGSubYingLiLength_2` varchar(255) COLLATE gb2312_bin DEFAULT NULL,
  `NGSubYingLiLength_3` varchar(255) COLLATE gb2312_bin DEFAULT NULL,
  `NGSubYingLiLength_4` varchar(255) COLLATE gb2312_bin DEFAULT NULL,
  `NGSubRadiusLength` varchar(255) COLLATE gb2312_bin DEFAULT NULL,
  `NGSubTotalLength` varchar(255) CHARACTER SET gb2312 COLLATE gb2312_chinese_ci DEFAULT NULL,
  `checktime` datetime DEFAULT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB AUTO_INCREMENT=80 DEFAULT CHARSET=gb2312 COLLATE=gb2312_bin;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `stickresult`
--

LOCK TABLES `stickresult` WRITE;
/*!40000 ALTER TABLE `stickresult` DISABLE KEYS */;
INSERT INTO `stickresult` VALUES (73,0,'C:/stickbmp/0/LJ_1.bmp','C:/stickbmp/0/LJ_2.bmp','C:/stickbmp/0/LJ_3.bmp','C:/stickbmp/0/LJ_4.bmp','C:/stickbmp/0/YinLie_1.bmp','C:/stickbmp/0/YinLie_2.bmp','C:/stickbmp/0/YinLie_3.bmp','C:/stickbmp/0/YinLie_4.bmp','C:/stickbmp/0/YingLi_1.bmp','C:/stickbmp/0/YingLi_2.bmp','C:/stickbmp/0/YingLi_3.bmp','C:/stickbmp/0/YingLi_4.bmp','5,10;35,40;','5,10;35,40;','5,10;35,40;','5,10;35,40;','16,30;50,70;','26,40;60,90;','26,40;60,90;','26,40;60,90;','16,30;50,70;','16,30;50,70;','16,30;50,70;','16,30;50,70;','26,40;60,90;','10,50;70,100;','2023-07-20 13:23:56'),(74,0,'C:/stickbmp/1/LJ_1.bmp','C:/stickbmp/1/LJ_2.bmp','C:/stickbmp/1/LJ_3.bmp','C:/stickbmp/1/LJ_4.bmp','C:/stickbmp/0/YinLie_1.bmp','C:/stickbmp/0/YinLie_2.bmp','C:/stickbmp/0/YinLie_3.bmp','C:/stickbmp/0/YinLie_4.bmp','C:/stickbmp/0/YingLi_1.bmp','C:/stickbmp/0/YingLi_2.bmp','C:/stickbmp/0/YingLi_3.bmp','C:/stickbmp/0/YingLi_4.bmp','5,10;35,40;','5,10;35,40;','5,10;35,40;','5,10;35,40;','16,30;50,70;','26,40;60,90;','26,40;60,90;','26,40;60,90;','16,30;50,70;','16,30;50,70;','16,30;50,70;','16,30;50,70;','26,40;60,90;','10,50;70,100;','2023-07-20 14:28:44'),(75,0,'C:/stickbmp/2/LJ_1.bmp','C:/stickbmp/2/LJ_2.bmp','C:/stickbmp/2/LJ_3.bmp','C:/stickbmp/2/LJ_4.bmp','C:/stickbmp/0/YinLie_1.bmp','C:/stickbmp/0/YinLie_2.bmp','C:/stickbmp/0/YinLie_3.bmp','C:/stickbmp/0/YinLie_4.bmp','C:/stickbmp/0/YingLi_1.bmp','C:/stickbmp/0/YingLi_2.bmp','C:/stickbmp/0/YingLi_3.bmp','C:/stickbmp/0/YingLi_4.bmp','5,10;35,40;','5,10;35,40;','5,10;35,40;','5,10;35,40;','16,30;50,70;','26,40;60,90;','26,40;60,90;','26,40;60,90;','16,30;50,70;','16,30;50,70;','16,30;50,70;','16,30;50,70;','26,40;60,90;','10,50;70,100;','2023-07-20 18:18:33'),(76,0,'C:/stickbmp/3/LJ_1.bmp','C:/stickbmp/3/LJ_2.bmp','C:/stickbmp/3/LJ_3.bmp','C:/stickbmp/3/LJ_4.bmp','C:/stickbmp/0/YinLie_1.bmp','C:/stickbmp/0/YinLie_2.bmp','C:/stickbmp/0/YinLie_3.bmp','C:/stickbmp/0/YinLie_4.bmp','C:/stickbmp/0/YingLi_1.bmp','C:/stickbmp/0/YingLi_2.bmp','C:/stickbmp/0/YingLi_3.bmp','C:/stickbmp/0/YingLi_4.bmp','5,10;35,40;','5,10;35,40;','5,10;35,40;','5,10;35,40;','16,30;50,70;','26,40;60,90;','26,40;60,90;','26,40;60,90;','16,30;50,70;','16,30;50,70;','16,30;50,70;','16,30;50,70;','26,40;60,90;','10,50;70,100;','2023-07-20 18:23:48'),(77,0,'C:/stickbmp/4/LJ_1.bmp','C:/stickbmp/4/LJ_2.bmp','C:/stickbmp/4/LJ_3.bmp','C:/stickbmp/4/LJ_4.bmp','C:/stickbmp/0/YinLie_1.bmp','C:/stickbmp/0/YinLie_2.bmp','C:/stickbmp/0/YinLie_3.bmp','C:/stickbmp/0/YinLie_4.bmp','C:/stickbmp/0/YingLi_1.bmp','C:/stickbmp/0/YingLi_2.bmp','C:/stickbmp/0/YingLi_3.bmp','C:/stickbmp/0/YingLi_4.bmp','5,10;35,40;','5,10;35,40;','5,10;35,40;','5,10;35,40;','16,30;50,70;','26,40;60,90;','26,40;60,90;','26,40;60,90;','16,30;50,70;','16,30;50,70;','16,30;50,70;','16,30;50,70;','26,40;60,90;','10,50;70,100;','2023-07-20 18:28:24'),(78,0,'C:/stickbmp/5/LJ_1.bmp','C:/stickbmp/5/LJ_2.bmp','C:/stickbmp/5/LJ_3.bmp','C:/stickbmp/5/LJ_4.bmp','C:/stickbmp/0/YinLie_1.bmp','C:/stickbmp/0/YinLie_2.bmp','C:/stickbmp/0/YinLie_3.bmp','C:/stickbmp/0/YinLie_4.bmp','C:/stickbmp/0/YingLi_1.bmp','C:/stickbmp/0/YingLi_2.bmp','C:/stickbmp/0/YingLi_3.bmp','C:/stickbmp/0/YingLi_4.bmp','5,10;35,40;','5,10;35,40;','5,10;35,40;','5,10;35,40;','16,30;50,70;','26,40;60,90;','26,40;60,90;','26,40;60,90;','16,30;50,70;','16,30;50,70;','16,30;50,70;','16,30;50,70;','26,40;60,90;','10,50;70,100;','2023-07-20 18:33:48'),(79,0,'C:/stickbmp/6/LJ_1.bmp','C:/stickbmp/6/LJ_2.bmp','C:/stickbmp/6/LJ_3.bmp','C:/stickbmp/6/LJ_4.bmp','C:/stickbmp/6/YinLie_1.bmp','C:/stickbmp/6/YinLie_2.bmp','C:/stickbmp/6/YinLie_3.bmp','C:/stickbmp/6/YinLie_4.bmp','C:/stickbmp/6/YingLi_1.bmp','C:/stickbmp/6/YingLi_2.bmp','C:/stickbmp/6/YingLi_3.bmp','C:/stickbmp/6/YingLi_4.bmp','5,10;35,40;','5,10;35,40;','5,10;35,40;','5,10;35,40;','16,30;50,70;','26,40;60,90;','26,40;60,90;','26,40;60,90;','16,30;50,70;','16,30;50,70;','16,30;50,70;','16,30;50,70;','26,40;60,90;','10,50;70,100;','2023-07-25 18:51:03');
/*!40000 ALTER TABLE `stickresult` ENABLE KEYS */;
UNLOCK TABLES;
/*!40103 SET TIME_ZONE=@OLD_TIME_ZONE */;

/*!40101 SET SQL_MODE=@OLD_SQL_MODE */;
/*!40014 SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS */;
/*!40014 SET UNIQUE_CHECKS=@OLD_UNIQUE_CHECKS */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
/*!40111 SET SQL_NOTES=@OLD_SQL_NOTES */;

-- Dump completed on 2023-07-26  9:21:17
