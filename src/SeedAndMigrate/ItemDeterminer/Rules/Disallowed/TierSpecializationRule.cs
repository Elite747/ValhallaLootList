// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using ValhallaLootList.Server.Data;

namespace ValhallaLootList.SeedAndMigrate.ItemDeterminer.Rules.Disallowed;

internal class TierSpecializationRule : SimpleRule
{
    private static readonly Dictionary<uint, Specializations> _tierSpecs = BuildTierSpecs();

    protected override string DisallowReason => "Tier gear is not for the chosen specialization.";

    protected override DeterminationLevel DisallowLevel => DeterminationLevel.Disallowed;

    protected override bool AppliesTo(Item item)
    {
        return _tierSpecs.ContainsKey(item.Id);
    }

    protected override bool IsAllowed(Item item, Specializations spec)
    {
        return (_tierSpecs[item.Id] & spec) != 0;
    }

    private static Dictionary<uint, Specializations> BuildTierSpecs()
    {
        var dictionary = new Dictionary<uint, Specializations>();

        // Warrior
        Add(Specializations.ArmsWarrior | Specializations.FuryWarrior, 39605, 39606, 39607, 39608, 39609, 40525, 40527, 40528, 40529, 40530); // Dreadnaught Battlegear
        Add(Specializations.ArmsWarrior | Specializations.FuryWarrior, 45429, 45430, 45431, 45432, 45433, 46146, 46148, 46149, 46150, 46151); // Siegebreaker Battlegear
        Add(Specializations.ArmsWarrior | Specializations.FuryWarrior, 48376, 48377, 48378, 48379, 48380, 48381, 48382, 48383, 48384, 48385); // Wrynn's Battlegear
        Add(Specializations.ArmsWarrior | Specializations.FuryWarrior, 48391, 48392, 48393, 48394, 48395, 48396, 48397, 48398, 48399, 48400); // Hellscream's Battlegear
        Add(Specializations.ArmsWarrior | Specializations.FuryWarrior, 51210, 51211, 51212, 51213, 51214, 51225, 51226, 51227, 51228, 51229); // Ymirjar Lord's Battlegear

        Add(Specializations.ProtWarrior, 39610, 39611, 39612, 39613, 39622, 40544, 40545, 40546, 40547, 40548); // Dreadnaught Plate
        Add(Specializations.ProtWarrior, 45424, 45425, 45426, 45427, 45428, 46162, 46164, 46166, 46167, 46169); // Siegebreaker Plate
        Add(Specializations.ProtWarrior, 48430, 48433, 48446, 48447, 48450, 48451, 48452, 48453, 48454, 48455); // Wrynn's Plate
        Add(Specializations.ProtWarrior, 48461, 48462, 48463, 48464, 48465, 48466, 48467, 48468, 48469, 48470); // Hellscream's Plate
        Add(Specializations.ProtWarrior, 51215, 51216, 51217, 51218, 51219, 51220, 51221, 51222, 51223, 51224); // Ymirjar Lord's Plate

        // Paladin
        Add(Specializations.HolyPaladin, 39628, 39629, 39630, 39631, 39632, 40569, 40570, 40571, 40572, 40573); // Redemption Regalia
        Add(Specializations.HolyPaladin, 45370, 45371, 45372, 45373, 45374, 46178, 46179, 46180, 46181, 46182); // Aegis Regalia
        Add(Specializations.HolyPaladin, 48575, 48576, 48577, 48578, 48579, 48580, 48581, 48582, 48583, 48584); // Turalyon's Garb
        Add(Specializations.HolyPaladin, 48585, 48586, 48587, 48588, 48589, 48590, 48591, 48592, 48593, 48594); // Liadrin's Garb
        Add(Specializations.HolyPaladin, 51165, 51166, 51167, 51168, 51169, 51270, 51271, 51272, 51273, 51274); // Lightsworn Garb

        Add(Specializations.RetPaladin, 39633, 39634, 39635, 39636, 39637, 40574, 40575, 40576, 40577, 40578); // Redemption Battlegear
        Add(Specializations.RetPaladin, 45375, 45376, 45377, 45379, 45380, 46152, 46153, 46154, 46155, 46156); // Aegis Battlegear
        Add(Specializations.RetPaladin, 48607, 48608, 48609, 48610, 48611, 48612, 48613, 48614, 48615, 48616); // Turalyon's Battlegear
        Add(Specializations.RetPaladin, 48617, 48618, 48619, 48620, 48621, 48622, 48623, 48624, 48625, 48626, 48630); // Liadrin's Battlegear
        Add(Specializations.RetPaladin, 51160, 51161, 51162, 51163, 51164, 51275, 51276, 51277, 51278, 51279); // Lightsworn Battlegear

        Add(Specializations.ProtPaladin, 39638, 39639, 39640, 39641, 39642, 40579, 40580, 40581, 40583, 40584); // Redemption Plate
        Add(Specializations.ProtPaladin, 45381, 45382, 45383, 45384, 45385, 46173, 46174, 46175, 46176, 46177); // Aegis Plate
        Add(Specializations.ProtPaladin, 48637, 48638, 48639, 48640, 48641, 48642, 48643, 48644, 48645, 48646); // Turalyon's Plate
        Add(Specializations.ProtPaladin, 48647, 48648, 48649, 48650, 48651, 48657, 48658, 48659, 48660, 48661); // Liadrin's Plate
        Add(Specializations.ProtPaladin, 51170, 51171, 51172, 51173, 51174, 51265, 51266, 51267, 51268, 51269); // Lightsworn Plate

        // Hunter
        //Add(SpecializationGroups.Hunter, 39578, 39579, 39580, 39581, 39582, 40503, 40504, 40505, 40506, 40507); // Cryptstalker Battlegear
        //Add(SpecializationGroups.Hunter, 45360, 45361, 45362, 45363, 45364, 46141, 46142, 46143, 46144, 46145); // Scourgestalker Battlegear
        //Add(SpecializationGroups.Hunter, 48255, 48256, 48257, 48258, 48259, 48260, 48261, 48262, 48263, 48264); // Windrunner's Battlegear
        //Add(SpecializationGroups.Hunter, 48265, 48266, 48267, 48268, 48269, 48270, 48271, 48272, 48273, 48274); // Windrunner's Pursuit
        //Add(SpecializationGroups.Hunter, 51150, 51151, 51152, 51153, 51154, 51285, 51286, 51287, 51288, 51289); // Ahn'Kahar Blood Hunter's Battlegear

        // Rogue
        //Add(SpecializationGroups.Rogue, 39558, 39560, 39561, 39564, 39565, 40495, 40496, 40499, 40500, 40502); // Bonescythe Battlegear
        //Add(SpecializationGroups.Rogue, 45396, 45397, 45398, 45399, 45400, 46123, 46124, 46125, 46126, 46127); // Terrorblade Battlegear
        //Add(SpecializationGroups.Rogue, 48223, 48224, 48225, 48226, 48227, 48228, 48229, 48230, 48231, 48232); // VanCleef's Battlegear
        //Add(SpecializationGroups.Rogue, 48233, 48234, 48235, 48236, 48237, 48238, 48239, 48240, 48241, 48242); // Garona's Battlegear
        //Add(SpecializationGroups.Rogue, 51185, 51186, 51187, 51188, 51189, 51250, 51251, 51252, 51253, 51254); // Shadowblade's Battlegear

        // Priest
        Add(SpecializationGroups.HealerPriest, 39514, 39515, 39517, 39518, 39519, 40445, 40447, 40448, 40449, 40450); // Regalia of Faith
        Add(SpecializationGroups.HealerPriest, 45386, 45387, 45388, 45389, 45390, 46188, 46190, 46193, 46195, 46197); // Sanctification Regalia
        Add(SpecializationGroups.HealerPriest, 47983, 47984, 47985, 47986, 47987, 48029, 48031, 48033, 48035, 48037); // Velen's Raiment
        Add(SpecializationGroups.HealerPriest, 48057, 48058, 48059, 48060, 48061, 48062, 48063, 48064, 48065, 48066); // Zabra's Raiment
        Add(SpecializationGroups.HealerPriest, 51175, 51176, 51177, 51178, 51179, 51260, 51261, 51262, 51263, 51264); // Crimson Acolyte's Raiment

        Add(Specializations.ShadowPriest, 39521, 39523, 39528, 39529, 39530, 40454, 40456, 40457, 40458, 40459); // Garb of Faith
        Add(Specializations.ShadowPriest, 45391, 45392, 45393, 45394, 45395, 46163, 46165, 46168, 46170, 46172); // Sanctification Garb
        Add(Specializations.ShadowPriest, 48077, 48078, 48079, 48080, 48081, 48082, 48083, 48084, 48085); // Velen's Regalia
        Add(Specializations.ShadowPriest, 48087, 48088, 48089, 48090, 48091, 48092, 48093, 48094, 48095, 48096); // Zabra's Regalia
        Add(Specializations.ShadowPriest, 51180, 51181, 51182, 51183, 51184, 51255, 51256, 51257, 51258, 51259); // Crimson Acolyte's Regalia

        // Death Knight
        Add(SpecializationGroups.DpsDeathKnight, 39617, 39618, 39619, 39620, 39621, 40550, 40552, 40554, 40556, 40557); // Scourgeborne Battlegear
        Add(SpecializationGroups.DpsDeathKnight, 45340, 45341, 45342, 45343, 45344, 46111, 46113, 46115, 46116, 46117); // Darkruned Battlegear
        Add(SpecializationGroups.DpsDeathKnight, 48481, 48482, 48483, 48484, 48485, 48486, 48487, 48488, 48489, 48490); // Thassarian's Battlegear
        Add(SpecializationGroups.DpsDeathKnight, 48491, 48492, 48493, 48494, 48495, 48496, 48497, 48498, 48499, 48500); // Koltira's Battlegear
        Add(SpecializationGroups.DpsDeathKnight, 51125, 51126, 51127, 51128, 51129, 51310, 51311, 51312, 51313, 51314); // Scourgelord's Battlegear

        Add(SpecializationGroups.TankDeathKnight, 39623, 39624, 39625, 39626, 39627, 40559, 40563, 40565, 40567, 40568); // Scourgeborne Plate
        Add(SpecializationGroups.TankDeathKnight, 45335, 45336, 45337, 45338, 45339, 46118, 46119, 46120, 46121, 46122); // Darkruned Plate
        Add(SpecializationGroups.TankDeathKnight, 48538, 48539, 48540, 48541, 48542, 48543, 48544, 48545, 48546, 48547); // Thassarian's Plate
        Add(SpecializationGroups.TankDeathKnight, 48548, 48549, 48550, 48551, 48552, 48553, 48554, 48555, 48556, 48557); // Koltira's Plate
        Add(SpecializationGroups.TankDeathKnight, 51130, 51131, 51132, 51133, 51134, 51305, 51306, 51307, 51308, 51309); // Scourgelord's Plate

        // Shaman
        Add(Specializations.RestoShaman, 39583, 39588, 39589, 39590, 39591, 40508, 40509, 40510, 40512, 40513); // Earthshatter Regalia
        Add(Specializations.RestoShaman, 45401, 45402, 45403, 45404, 45405, 46198, 46199, 46201, 46202, 46204); // Worldbreaker Regalia
        Add(Specializations.RestoShaman, 48285, 48286, 48287, 48288, 48289, 48290, 48291, 48292, 48293, 48294); // Nobundo's Garb
        Add(Specializations.RestoShaman, 48300, 48301, 48302, 48303, 48304, 48305, 48306, 48307, 48308, 48309); // Thrall's Garb
        Add(Specializations.RestoShaman, 51190, 51191, 51192, 51193, 51194, 51245, 51246, 51247, 51248, 51249); // Frost Witch's Garb

        Add(Specializations.EleShaman, 39592, 39593, 39594, 39595, 39596, 40514, 40515, 40516, 40517, 40518); // Earthshatter Garb
        Add(Specializations.EleShaman, 45406, 45408, 45409, 45410, 45411, 46206, 46207, 46209, 46210, 46211); // Worldbreaker Garb
        Add(Specializations.EleShaman, 48316, 48317, 48318, 48319, 48320, 48321, 48322, 48323, 48324, 48325); // Nobundo's Regalia
        Add(Specializations.EleShaman, 48326, 48327, 48328, 48329, 48330, 48331, 48332, 48333, 48334, 48335); // Thrall's Regalia
        Add(Specializations.EleShaman, 51200, 51201, 51202, 51203, 51204, 51235, 51236, 51237, 51238, 51239); // Frost Witch's Regalia

        Add(Specializations.EnhanceShaman, 39597, 39601, 39602, 39603, 39604, 40520, 40521, 40522, 40523, 40524); // Earthshatter Battlegear
        Add(Specializations.EnhanceShaman, 45412, 45413, 45414, 45415, 45416, 46200, 46203, 46205, 46208, 46212); // Worldbreaker Battlegear
        Add(Specializations.EnhanceShaman, 48346, 48347, 48348, 48349, 48350, 48351, 48352, 48353, 48354, 48355); // Nobundo's Battlegear
        Add(Specializations.EnhanceShaman, 48356, 48357, 48358, 48359, 48360, 48361, 48362, 48363, 48364, 48365); // Thrall's Battlegear
        Add(Specializations.EnhanceShaman, 51195, 51196, 51197, 51198, 51199, 51240, 51241, 51242, 51243, 51244); // Frost Witch's Battlegear

        // Mage
        //Add(SpecializationGroups.Mage, 39491, 39492, 39493, 39494, 39495, 40415, 40416, 40417, 40418, 40419); // Frostfire Garb
        //Add(SpecializationGroups.Mage, 45365, 45367, 45368, 45369, 46129, 46130, 46131, 46132, 46133, 46134); // Kirin Tor Garb
        //Add(SpecializationGroups.Mage, 47753, 47754, 47755, 47756, 47757, 47758, 47759, 47760, 47761, 47762); // Khadgar's Regalia
        //Add(SpecializationGroups.Mage, 47763, 47764, 47765, 47766, 47767, 47768, 47769, 47770, 47771, 47772); // Sunstrider's Regalia
        //Add(SpecializationGroups.Mage, 51155, 51156, 51157, 51158, 51159, 51280, 51281, 51282, 51283, 51284); // Bloodmage's Regalia

        // Warlock
        //Add(SpecializationGroups.Warlock, 39496, 39497, 39498, 39499, 39500, 40420, 40421, 40422, 40423, 40424); // Plagueheart Garb
        //Add(SpecializationGroups.Warlock, 45417, 45419, 45420, 45421, 45422, 46135, 46136, 46137, 46139, 46140); // Deathbringer Garb
        //Add(SpecializationGroups.Warlock, 47778, 47779, 47780, 47781, 47782, 47788, 47789, 47790, 47791, 47792); // Kel'Thuzad's Regalia
        //Add(SpecializationGroups.Warlock, 47793, 47794, 47795, 47796, 47797, 47803, 47804, 47805, 47806, 47807); // Gul'dan's Regalia
        //Add(SpecializationGroups.Warlock, 51205, 51206, 51207, 51208, 51209, 51230, 51231, 51232, 51233, 51234); // Dark Coven's Regalia

        // Druid
        Add(Specializations.RestoDruid, 39531, 39538, 39539, 39542, 39543, 40460, 40461, 40462, 40463, 40465); // Dreamwalker Regalia
        Add(Specializations.RestoDruid, 45345, 45346, 45347, 45348, 45349, 46183, 46184, 46185, 46186, 46187); // Nightsong Regalia
        Add(Specializations.RestoDruid, 48133, 48134, 48135, 48136, 48137, 48138, 48139, 48140, 48141, 48142); // Malfurion's Garb
        Add(Specializations.RestoDruid, 48143, 48144, 48145, 48146, 48147, 48148, 48149, 48150, 48151, 48152); // Runetotem's Garb
        Add(Specializations.RestoDruid, 51135, 51136, 51137, 51138, 51139, 51300, 51301, 51302, 51303, 51304); // Lasherweave Garb

        Add(Specializations.BalanceDruid, 39544, 39545, 39546, 39547, 39548, 40466, 40467, 40468, 40469, 40470); // Dreamwalker Garb
        Add(Specializations.BalanceDruid, 45351, 45352, 45353, 45354, 46189, 46191, 46192, 46194, 46196, 46313); // Nightsong Garb
        Add(Specializations.BalanceDruid, 48163, 48164, 48165, 48166, 48167, 48168, 48169, 48170, 48171, 48172); // Malfurion's Regalia
        Add(Specializations.BalanceDruid, 48173, 48174, 48175, 48176, 48177, 48178, 48179, 48180, 48181, 48182); // Runetotem's Regalia
        Add(Specializations.BalanceDruid, 51145, 51146, 51147, 51148, 51149, 51290, 51291, 51292, 51293, 51294); // Lasherweave Regalia

        Add(Specializations.BearDruid | Specializations.CatDruid, 39553, 39554, 39555, 39556, 39557, 40471, 40472, 40473, 40493, 40494); // Dreamwalker Battlegear
        Add(Specializations.BearDruid | Specializations.CatDruid, 45355, 45356, 45357, 45358, 45359, 46157, 46158, 46159, 46160, 46161); // Nightsong Battlegear
        Add(Specializations.BearDruid | Specializations.CatDruid, 48193, 48194, 48195, 48196, 48197, 48198, 48199, 48200, 48201, 48202); // Runetotem's Battlegear
        Add(Specializations.BearDruid | Specializations.CatDruid, 48203, 48204, 48205, 48206, 48207, 48208, 48209, 48210, 48211, 48212); // Malfurion's Battlegear
        Add(Specializations.BearDruid | Specializations.CatDruid, 51140, 51141, 51142, 51143, 51144, 51295, 51296, 51297, 51298, 51299); // Lasherweave Battlegear

        return dictionary;

        void Add(Specializations spec, params uint[] items)
        {
            foreach (var item in items)
            {
                dictionary[item] = spec;
            }
        }
    }
}
