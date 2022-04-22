DROP TABLE IF EXISTS `bests`;
CREATE TABLE `bests` (
  `user_id` int(11) NOT NULL,
  `song_id` varchar(255) COLLATE utf8mb4_unicode_ci NOT NULL,
  `difficulty` int(11) NOT NULL,
  `score` int(11) DEFAULT NULL,
  `shiny_perfect_count` int(11) DEFAULT NULL,
  `perfect_count` int(11) DEFAULT NULL,
  `near_count` int(11) DEFAULT NULL,
  `miss_count` int(11) DEFAULT NULL,
  `health` int(11) DEFAULT NULL,
  `modifier` int(11) DEFAULT NULL,
  `time_played` int(11) DEFAULT NULL,
  `best_clear_type` int(11) DEFAULT NULL,
  `clear_type` int(11) DEFAULT NULL,
  `rating` decimal(10,3) DEFAULT NULL,
  PRIMARY KEY (`user_id`,`song_id`,`difficulty`) USING BTREE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci ROW_FORMAT=DYNAMIC;
DROP TABLE IF EXISTS `bests_special`;
CREATE TABLE `bests_special` (
  `user_id` int(11) NOT NULL,
  `song_id` varchar(255) COLLATE utf8mb4_unicode_ci NOT NULL,
  `difficulty` int(11) NOT NULL,
  `score` int(11) DEFAULT NULL,
  `shiny_perfect_count` int(11) DEFAULT NULL,
  `perfect_count` int(11) DEFAULT NULL,
  `near_count` int(11) DEFAULT NULL,
  `miss_count` int(11) DEFAULT NULL,
  `health` int(11) DEFAULT NULL,
  `modifier` int(11) DEFAULT NULL,
  `time_played` int(11) DEFAULT NULL,
  `best_clear_type` int(11) DEFAULT NULL,
  `clear_type` int(11) DEFAULT NULL,
  `rating` decimal(10,3) DEFAULT NULL,
  PRIMARY KEY (`user_id`,`song_id`,`difficulty`) USING BTREE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci ROW_FORMAT=DYNAMIC;
DROP TABLE IF EXISTS `fixed_characters`;
CREATE TABLE `fixed_characters` (
  `character_id` int(11) NOT NULL,
  `character_nameid` mediumtext COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `name` mediumtext COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `is_available` int(11) DEFAULT 1,
  `maxLevel` int(11) DEFAULT NULL,
  `level_exps` mediumtext COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `frag` mediumtext COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `prog` mediumtext COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `overdrive` mediumtext COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `skill_id` mediumtext COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `skill_unlock_level` int(11) DEFAULT 1,
  `skill_requires_uncap` int(11) DEFAULT 0,
  `skill_id_uncap` mediumtext COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `char_type` int(11) DEFAULT 0 COMMENT '0=平衡型 1=支援型 2=挑战型',
  `uncap_cores` mediumtext COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `is_uncapped` int(11) DEFAULT 0,
  `is_uncapped_override` int(11) DEFAULT 0,
  `ordered_id` int(10) unsigned NOT NULL DEFAULT 0,
  `version` varchar(255) COLLATE utf8mb4_unicode_ci NOT NULL,
  PRIMARY KEY (`character_id`) USING BTREE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci ROW_FORMAT=DYNAMIC;
DROP TABLE IF EXISTS `fixed_packs`;
CREATE TABLE `fixed_packs` (
  `id` int(10) unsigned NOT NULL AUTO_INCREMENT,
  `pid` varchar(255) COLLATE utf8mb4_unicode_ci NOT NULL,
  `name_en` varchar(255) COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `date` int(11) DEFAULT 0,
  `is_available` int(11) DEFAULT 1,
  `price` int(11) NOT NULL DEFAULT -1,
  `original_price` int(11) NOT NULL DEFAULT -1,
  `discount_from` datetime DEFAULT NULL,
  `discount_to` datetime DEFAULT NULL,
  `custom_banner` varchar(255) COLLATE utf8mb4_unicode_ci DEFAULT '0',
  `img` varchar(255) COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `plus_character` int(11) DEFAULT -1,
  `description_en` varchar(1024) COLLATE utf8mb4_unicode_ci DEFAULT '',
  `description_ja` varchar(1024) COLLATE utf8mb4_unicode_ci DEFAULT '',
  `description_zh_Hans` varchar(1024) COLLATE utf8mb4_unicode_ci DEFAULT '',
  `description_zh_Hant` varchar(1024) COLLATE utf8mb4_unicode_ci DEFAULT '',
  `source_en` varchar(255) COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `source_ja` varchar(255) COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `source_zh_Hans` varchar(255) COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `source_zh_Hant` varchar(255) COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `copyright` varchar(255) COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `version` mediumtext COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  PRIMARY KEY (`id`,`pid`) USING BTREE
) ENGINE=InnoDB AUTO_INCREMENT=100 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci ROW_FORMAT=DYNAMIC;
DROP TABLE IF EXISTS `fixed_presents`;
CREATE TABLE `fixed_presents` (
  `present_id` varchar(255) COLLATE utf8mb4_unicode_ci NOT NULL,
  `description_en` varchar(255) COLLATE utf8mb4_unicode_ci NOT NULL,
  `description_ja` varchar(255) COLLATE utf8mb4_unicode_ci NOT NULL,
  `description_zh-Hans` varchar(255) COLLATE utf8mb4_unicode_ci NOT NULL,
  `description_zh-Hant` varchar(255) COLLATE utf8mb4_unicode_ci NOT NULL,
  `description_ko` varchar(255) COLLATE utf8mb4_unicode_ci NOT NULL,
  `expire_time` datetime NOT NULL,
  `items` text COLLATE utf8mb4_unicode_ci NOT NULL,
  PRIMARY KEY (`present_id`) USING BTREE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci ROW_FORMAT=DYNAMIC;
DROP TABLE IF EXISTS `fixed_properties`;
CREATE TABLE `fixed_properties` (
  `key` varchar(255) COLLATE utf8mb4_unicode_ci NOT NULL,
  `value` mediumtext COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  PRIMARY KEY (`key`) USING BTREE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci ROW_FORMAT=DYNAMIC;
DROP TABLE IF EXISTS `fixed_purchases`;
CREATE TABLE `fixed_purchases` (
  `item_id` varchar(255) COLLATE utf8mb4_unicode_ci NOT NULL,
  `item_type` varchar(255) COLLATE utf8mb4_unicode_ci NOT NULL DEFAULT 'single',
  `is_available` tinyint(4) NOT NULL DEFAULT 1,
  `price` int(11) NOT NULL DEFAULT 100,
  `original_price` int(11) DEFAULT 100,
  `discount_from` datetime DEFAULT NULL,
  `discount_to` datetime DEFAULT NULL,
  PRIMARY KEY (`item_id`) USING BTREE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci ROW_FORMAT=DYNAMIC;
DROP TABLE IF EXISTS `fixed_songs`;
CREATE TABLE `fixed_songs` (
  `sid` varchar(255) COLLATE utf8mb4_unicode_ci NOT NULL,
  `name_en` varchar(255) COLLATE utf8mb4_unicode_ci NOT NULL,
  `name_jp` varchar(255) COLLATE utf8mb4_unicode_ci DEFAULT '',
  `bpm` varchar(255) COLLATE utf8mb4_unicode_ci NOT NULL DEFAULT '',
  `bpm_base` int(11) NOT NULL DEFAULT 0,
  `pakset` varchar(255) COLLATE utf8mb4_unicode_ci NOT NULL DEFAULT '',
  `artist` varchar(255) COLLATE utf8mb4_unicode_ci NOT NULL DEFAULT '',
  `time` int(11) NOT NULL DEFAULT 0,
  `side` int(11) NOT NULL DEFAULT 0,
  `date` int(11) NOT NULL DEFAULT 0,
  `world_unlock` varchar(255) COLLATE utf8mb4_unicode_ci NOT NULL DEFAULT '0',
  `remote_download` varchar(255) COLLATE utf8mb4_unicode_ci NOT NULL DEFAULT '0',
  `rating_pst` int(11) NOT NULL DEFAULT 0,
  `rating_prs` int(11) NOT NULL DEFAULT 0,
  `rating_ftr` int(11) NOT NULL DEFAULT 0,
  `rating_byd` int(11) NOT NULL DEFAULT 0,
  `difficulty_pst` int(11) NOT NULL DEFAULT 0,
  `difficulty_prs` int(11) NOT NULL DEFAULT 0,
  `difficulty_ftr` int(11) NOT NULL DEFAULT 0,
  `difficulty_byd` int(11) NOT NULL DEFAULT 0,
  `chart_designer_pst` varchar(255) COLLATE utf8mb4_unicode_ci NOT NULL DEFAULT '',
  `chart_designer_prs` varchar(255) COLLATE utf8mb4_unicode_ci NOT NULL DEFAULT '',
  `chart_designer_ftr` varchar(255) COLLATE utf8mb4_unicode_ci NOT NULL DEFAULT '',
  `chart_designer_byd` varchar(255) COLLATE utf8mb4_unicode_ci NOT NULL DEFAULT '',
  `jacket_designer_pst` varchar(255) COLLATE utf8mb4_unicode_ci NOT NULL DEFAULT '',
  `jacket_designer_prs` varchar(255) COLLATE utf8mb4_unicode_ci NOT NULL DEFAULT '',
  `jacket_designer_ftr` varchar(255) COLLATE utf8mb4_unicode_ci NOT NULL DEFAULT '',
  `jacket_designer_byd` varchar(255) COLLATE utf8mb4_unicode_ci NOT NULL DEFAULT '',
  `purchase` varchar(255) COLLATE utf8mb4_unicode_ci NOT NULL DEFAULT '',
  `audioPreview` int(11) NOT NULL DEFAULT 0,
  `audioPreviewEnd` int(11) DEFAULT 0,
  `bg` varchar(255) COLLATE utf8mb4_unicode_ci NOT NULL DEFAULT '',
  `source_en` varchar(255) COLLATE utf8mb4_unicode_ci NOT NULL DEFAULT '',
  `source_jp` varchar(255) COLLATE utf8mb4_unicode_ci NOT NULL DEFAULT '',
  `source_copyright` varchar(255) COLLATE utf8mb4_unicode_ci NOT NULL DEFAULT '',
  `unlock_key_append_pst` varchar(255) COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `unlock_key_append_prs` varchar(255) COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `unlock_key_append_ftr` varchar(255) COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `unlock_key_append_byd` varchar(255) COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `version` varchar(255) COLLATE utf8mb4_unicode_ci NOT NULL,
  PRIMARY KEY (`sid`,`date`) USING BTREE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci ROW_FORMAT=DYNAMIC;
DROP TABLE IF EXISTS `fixed_songs_checksum`;
CREATE TABLE `fixed_songs_checksum` (
  `sid` varchar(255) COLLATE utf8mb4_unicode_ci NOT NULL,
  `filename` varchar(255) COLLATE utf8mb4_unicode_ci NOT NULL,
  `checksum` varchar(255) COLLATE utf8mb4_unicode_ci NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci ROW_FORMAT=DYNAMIC;
DROP TABLE IF EXISTS `friend`;
CREATE TABLE `friend` (
  `user_id_me` int(11) NOT NULL,
  `user_id_other` int(11) NOT NULL,
  PRIMARY KEY (`user_id_me`,`user_id_other`) USING BTREE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci ROW_FORMAT=DYNAMIC;
DROP TABLE IF EXISTS `logins`;
CREATE TABLE `logins` (
  `access_token` varchar(255) COLLATE utf8mb4_unicode_ci NOT NULL,
  `user_id` int(11) DEFAULT NULL,
  `last_login_time` int(11) DEFAULT NULL,
  `last_login_ip` mediumtext COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `last_login_device` mediumtext COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `last_login_deviceId` mediumtext COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  PRIMARY KEY (`access_token`) USING BTREE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci ROW_FORMAT=DYNAMIC;
DROP TABLE IF EXISTS `user_bydunlocks`;
CREATE TABLE `user_bydunlocks` (
  `user_id` int(11) NOT NULL,
  `sid` varchar(255) NOT NULL,
  PRIMARY KEY (`user_id`,`sid`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;
DROP TABLE IF EXISTS `user_chars`;
CREATE TABLE `user_chars` (
  `user_id` int(11) NOT NULL,
  `character_id` int(11) NOT NULL,
  `level` int(11) DEFAULT 1,
  `exp` int(11) DEFAULT NULL,
  `level_exp` int(11) DEFAULT NULL,
  `frag` int(11) DEFAULT 50,
  `prog` int(11) DEFAULT 50,
  `overdrive` int(11) DEFAULT 50,
  `skill_id` mediumtext COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `skill_unlock_level` int(11) DEFAULT 1,
  `skill_requires_uncap` int(11) DEFAULT 0,
  `skill_id_uncap` mediumtext COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `char_type` int(11) DEFAULT 0,
  `is_uncapped` int(11) DEFAULT 0,
  `is_uncapped_override` int(11) DEFAULT 0,
  PRIMARY KEY (`user_id`,`character_id`) USING BTREE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci ROW_FORMAT=DYNAMIC;
DROP TABLE IF EXISTS `user_saves`;
CREATE TABLE `user_saves` (
  `user_id` int(10) unsigned NOT NULL,
  `scores_data` longtext COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `clearlamps_data` longtext COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `clearedsongs_data` longtext COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `unlocklist_data` longtext COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `story_data` longtext COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `device_id` varchar(255) COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `device_name` varchar(255) COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `create_time` datetime DEFAULT NULL,
  PRIMARY KEY (`user_id`) USING BTREE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci ROW_FORMAT=DYNAMIC;
DROP TABLE IF EXISTS `user_world`;
CREATE TABLE `user_world` (
  `user_id` int(11) NOT NULL,
  `map_id` varchar(255) COLLATE utf8mb4_unicode_ci NOT NULL,
  `curr_position` int(11) DEFAULT 0,
  `curr_capture` double DEFAULT 0,
  `is_locked` int(11) DEFAULT 0,
  PRIMARY KEY (`user_id`,`map_id`) USING BTREE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci ROW_FORMAT=DYNAMIC;
DROP TABLE IF EXISTS `users`;
CREATE TABLE `users` (
  `user_id` int(11) NOT NULL AUTO_INCREMENT,
  `user_code` char(10) COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `name` varchar(255) COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `password` mediumtext COLLATE utf8mb4_unicode_ci NOT NULL,
  `join_date` bigint(20) DEFAULT NULL,
  `user_rating` int(11) DEFAULT 0,
  `character_id` int(11) DEFAULT 0,
  `is_skill_sealed` int(11) DEFAULT 0,
  `is_char_uncapped` int(11) DEFAULT 0,
  `is_char_uncapped_override` int(11) DEFAULT 0,
  `is_hide_rating` int(11) DEFAULT 0,
  `song_id` mediumtext COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `difficulty` int(11) DEFAULT NULL,
  `score` int(11) DEFAULT NULL,
  `shiny_perfect_count` int(11) DEFAULT NULL,
  `perfect_count` int(11) DEFAULT NULL,
  `near_count` int(11) DEFAULT NULL,
  `miss_count` int(11) DEFAULT NULL,
  `health` int(11) DEFAULT NULL,
  `modifier` int(11) DEFAULT NULL,
  `time_played` int(11) DEFAULT NULL,
  `clear_type` int(11) DEFAULT NULL,
  `rating` decimal(10,3) DEFAULT 0.000,
  `favorite_character` int(11) DEFAULT -1,
  `max_stamina_notification_enabled` int(11) DEFAULT 1,
  `current_map` mediumtext COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `cloud_device_id` mediumtext COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `cloud_device_name` mediumtext COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `cloud_data` mediumtext COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `ticket` int(11) DEFAULT 0,
  `world_time_fullrecharged` datetime DEFAULT NULL,
  `is_banned` int(11) DEFAULT 0,
  `email` varchar(255) COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `credit_point` int(11) NOT NULL DEFAULT 12,
  `credit_edit_reasons` longtext COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `purchases` longtext COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `claimed_presents` text COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `overflow_staminas` int(11) NOT NULL DEFAULT 0,
  `world_songs` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL,
  `totalScore` bigint(20) unsigned NOT NULL DEFAULT 0,
  PRIMARY KEY (`user_id`) USING BTREE,
  UNIQUE KEY `name` (`name`) USING BTREE
) ENGINE=InnoDB AUTO_INCREMENT=10000024 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci ROW_FORMAT=DYNAMIC;
DROP TABLE IF EXISTS `world_songplay`;
CREATE TABLE `world_songplay` (
  `user_id` int(11) NOT NULL,
  `song_id` varchar(255) COLLATE utf8mb4_unicode_ci NOT NULL,
  `difficulty` int(11) NOT NULL,
  `stamina_multiply` int(11) DEFAULT NULL,
  `fragment_multiply` int(11) DEFAULT NULL,
  `prog_boost_multiply` int(11) DEFAULT NULL,
  PRIMARY KEY (`user_id`,`song_id`,`difficulty`) USING BTREE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci ROW_FORMAT=DYNAMIC;
DROP TABLE IF EXISTS `bots`;
CREATE TABLE `bots`  (
  `id` int NOT NULL AUTO_INCREMENT,
  `apikey` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
  `name` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
  `descrption` text CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL,
  `botQQ` bigint NOT NULL,
  `author` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
  `authorQQ` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
  `is_banned` tinyint(1) NOT NULL DEFAULT 0,
  PRIMARY KEY (`id`) USING BTREE,
  UNIQUE INDEX `UniqueApikey`(`apikey`) USING BTREE
) ENGINE = InnoDB AUTO_INCREMENT = 2 CHARACTER SET = utf8mb4 COLLATE = utf8mb4_unicode_ci ROW_FORMAT = Dynamic;
INSERT INTO `fixed_properties` (`key`, `value`) VALUES ('core_exp', '250');
INSERT INTO `fixed_properties` (`key`, `value`) VALUES ('is_byd_chapter_unlocked', '1');
INSERT INTO `fixed_properties` (`key`, `value`) VALUES ('level_steps', '[50,100,150,200,250,300,400,500,800,1000,2000,3000,5000,7000,10000,12000,14000,16000,18000,20000,21000,22000,23000,24000,25000,26000,27000,28000,29000,30000]');
INSERT INTO `fixed_properties` (`key`, `value`) VALUES ('max_stamina', '12');
INSERT INTO `fixed_properties` (`key`, `value`) VALUES ('world_ranking_enabled', '1');