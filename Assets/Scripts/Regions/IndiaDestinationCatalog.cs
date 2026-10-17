using System.Collections.Generic;
using UnityEngine;

namespace LensOfLegends.Regions
{
    /// <summary>
    /// Builds and provides the India-wide catalog of destinations.
    /// Ensures Theyyam is Chapter 1 (fully playable vertical slice) while Varanasi, Rajasthan,
    /// Kolkata, Nagaland, and Ladakh are structured with distinct visual identities, palettes,
    /// and verified cultural notes, clearly marked as Preview / Future Destinations.
    /// </summary>
    public static class IndiaDestinationCatalog
    {
        private static List<RegionData> _cachedRegions;

        public static List<RegionData> GetAllDestinations()
        {
            if (_cachedRegions != null && _cachedRegions.Count > 0 && _cachedRegions[0] != null)
                return _cachedRegions;

            _cachedRegions = new List<RegionData>
            {
                CreateNorthKeralaTheyyam(),
                CreateVaranasiSacredRiverfront(),
                CreateRajasthanDesertFestival(),
                CreateKolkataDurgaPuja(),
                CreateNagalandHornbillFestival(),
                CreateLadakhMonasticFestivals()
            };

            return _cachedRegions;
        }

        public static RegionData GetDestinationById(string regionId)
        {
            List<RegionData> all = GetAllDestinations();
            for (int i = 0; i < all.Count; i++)
            {
                if (all[i].regionId == regionId)
                    return all[i];
            }
            return all[0];
        }

        private static RegionData CreateNorthKeralaTheyyam()
        {
            RegionData r = ScriptableObject.CreateInstance<RegionData>();
            r.name = "Region_01_NorthKerala_Theyyam";
            r.regionId = "kerala_theyyam";
            r.regionTitle = "North Kerala — Theyyam in the Sacred Grove";
            r.stateOrTerritory = "Kerala (North Malabar — Kannur & Kasaragod)";
            r.chapterSubtitle = "Chapter I: Firelight of the Kavu (Playable Vertical Slice)";
            r.playableState = DestinationPlayableState.PlayableVerticalSlice;
            r.targetSceneName = "Theyyam_Ritual_Scene";
            r.normalizedIndiaMapPosition = new Vector2(0.32f, 0.14f);

            r.themeSummary = "Living ritual traditions, sacred groves (Kavu), firelight, Chenda percussion, laterite shrine architecture, and nighttime atmosphere.";
            r.regionalIntroductionNarrative =
                "[Narrative Story Intro] You arrive at dusk in a humid village courtyard in North Malabar. " +
                "Local mentor and archivist Madhavan Nair welcomes you beside the laterite wall of an ancestral Kavu (sacred grove). " +
                "Oil lamps (Nilavilakku) and a ceremonial bonfire (Meleri) illuminate the red earth as the rhythmic pulse of the Chenda drum echoes through coconut and areca palms.";
            r.verifiedCulturalBackground =
                "[Verified Cultural Fact] Theyyam (also known as Kaliyattam) is a centuries-old living ritual worship tradition practiced across sacred groves (Kavus) and village shrines in the North Malabar region of Kerala (primarily Kannur and Kasaragod districts) between December and May. " +
                "During the ritual, following the vocal Thottam invocation and intricate Mukhathezhuthu facial painting with natural pigments, the performer is revered by the community not merely as an actor, but as a living manifestation of the deity or ancestral hero.";
            r.primaryReferenceUrl = "https://en.wikipedia.org/wiki/Theyyam";

            r.skyTopColor = new Color(0.03f, 0.05f, 0.11f);
            r.skyHorizonColor = new Color(0.18f, 0.08f, 0.05f);
            r.ambientLightColor = new Color(0.22f, 0.15f, 0.12f);
            r.keyLightColor = new Color(1.0f, 0.56f, 0.22f);
            r.accentUiColor = new Color(0.94f, 0.42f, 0.18f);
            r.fogDensity = 0.016f;
            r.fogColor = new Color(0.08f, 0.05f, 0.05f);

            r.soundscapeProfileId = "kerala_night_kavu";
            r.ambientSoundscapeDescription = "Synthesized Malabar night crickets, crackling Meleri fire embers, gentle tropical breeze, and rhythmic Chenda-inspired percussion pulse (procedural audio placeholder).";
            r.baseWindIntensity = 0.25f;
            r.nightInsectsIntensity = 0.75f;
            r.percussionRhythmTempoBpm = 104f;

            r.plannedPhotographySubjects = new List<string>
            {
                "Theyyam Performer — Towered Mudi Headdress & Facial Art",
                "Meleri Ritual Bonfire & Rising Embers",
                "Nilavilakku Traditional Bronze Oil Lamps",
                "Kavu Laterite Stone Shrine & Wooden Gables",
                "Chenda & Elathalam Percussion Ensemble Area",
                "Sacred Banyan Tree & Palm Canopy Silhouette"
            };

            r.pointsOfInterest = new List<CulturalPointOfInterestDefinition>
            {
                new CulturalPointOfInterestDefinition
                {
                    poiId = "poi_theyyam_performer",
                    title = "The Living Deity: Theyyam (Kaliyattam) Performance",
                    category = "Rituals & Performance",
                    verifiedCulturalInformation =
                        "Verified Cultural Context: Practiced in North Malabar's Kannur and Kasaragod districts, Theyyam encompasses over 400 distinct forms. Performers wear towering bamboo-and-palm-leaf headgear (Mudi), intricately painted facial patterns (Mukhathezhuthu), and red garments, dancing before the shrine fire.",
                    verifiedReferenceSource = "https://en.wikipedia.org/wiki/Theyyam",
                    narrativeStorytellerNote =
                        "Storyteller Madhavan's Note (Fictional Narrative): 'Watch how the firelight catches the vermilion pigment when the performer turns toward the shrine. Try a fast shutter speed to freeze the spin, or slow it down to let the embers streak across your frame.'",
                    worldPosition = new Vector3(0f, 1.5f, 4.0f),
                    discoveryRadius = 8.5f
                },
                new CulturalPointOfInterestDefinition
                {
                    poiId = "poi_kavu_shrine",
                    title = "Kavu: The Sacred Grove & Laterite Shrine",
                    category = "Architecture & Ecology",
                    verifiedCulturalInformation =
                        "Verified Cultural Context: A Kavu is a traditional sacred grove on the Malabar Coast of Kerala that preserves local biodiversity alongside ancestral and folk shrines built from porous red laterite stone and timber.",
                    verifiedReferenceSource = "https://en.wikipedia.org/wiki/Kavu",
                    narrativeStorytellerNote =
                        "Storyteller Madhavan's Note (Fictional Narrative): 'Before brick and concrete, our ancestors carved red laterite straight from the earth. Step back with a 28mm wide lens to frame the shrine roof against the banyan branches.'",
                    worldPosition = new Vector3(0f, 1.8f, 15.5f),
                    discoveryRadius = 7.5f
                },
                new CulturalPointOfInterestDefinition
                {
                    poiId = "poi_nilavilakku_lamps",
                    title = "Nilavilakku & Meleri: Sacred Firelight",
                    category = "Rituals & Crafts",
                    verifiedCulturalInformation =
                        "Verified Cultural Context: Traditional cast-bronze standing oil lamps (Nilavilakku) and the ceremonial Meleri fire pit illuminate nighttime Kavu rituals, providing the warm, high-contrast lighting characteristic of North Kerala performances.",
                    verifiedReferenceSource = "https://www.theyyakkalam.in/glossary",
                    narrativeStorytellerNote =
                        "Storyteller Madhavan's Note (Fictional Narrative): 'Night photography here is a dialogue with shadow. Dial your Exposure Compensation slightly negative (-0.5 EV) so the bronze flames glow without blowing out.'",
                    worldPosition = new Vector3(-4.2f, 1.2f, 5.2f),
                    discoveryRadius = 6.0f
                },
                new CulturalPointOfInterestDefinition
                {
                    poiId = "poi_chenda_ensemble",
                    title = "Chenda Melam & Thottam Invocation",
                    category = "Music & Performance",
                    verifiedCulturalInformation =
                        "Verified Cultural Context: A Theyyam ritual begins with the Thottam—a ritual song narrating the legend of the deity—accompanied by cylindrical wooden Chenda drums and bronze cymbals that build rhythmic intensity through the night.",
                    verifiedReferenceSource = "https://en.wikipedia.org/wiki/Theyyam",
                    narrativeStorytellerNote =
                        "Storyteller Madhavan's Note (Fictional Narrative): 'Listen to the cadence of the drums. Every crescendo signals a leap or turn in the courtyard.'",
                    worldPosition = new Vector3(6.8f, 1.4f, 6.0f),
                    discoveryRadius = 6.5f
                },
                new CulturalPointOfInterestDefinition
                {
                    poiId = "poi_sacred_banyan",
                    title = "Ancient Banyan & Areca Palm Canopy",
                    category = "Natural Landscapes",
                    verifiedCulturalInformation =
                        "Verified Cultural Context: Sacred groves in North Kerala act as community-protected micro-forests, sheltering ancient Ficus (banyan/peepal) trees, medicinal plants, and nocturnal wildlife.",
                    verifiedReferenceSource = "https://ecoheritage.cpreec.org/sacred-groves-of-north-kerala/",
                    narrativeStorytellerNote =
                        "Storyteller Madhavan's Note (Fictional Narrative): 'Walk to the western edge of the grove and frame the performer through the hanging banyan roots for natural foreground depth.'",
                    worldPosition = new Vector3(-9.5f, 2.0f, 11.0f),
                    discoveryRadius = 7.0f
                }
            };

            return r;
        }

        private static RegionData CreateVaranasiSacredRiverfront()
        {
            RegionData r = ScriptableObject.CreateInstance<RegionData>();
            r.name = "Region_02_Varanasi_Riverfront";
            r.regionId = "varanasi_ghats";
            r.regionTitle = "Varanasi — The Sacred Riverfront";
            r.stateOrTerritory = "Uttar Pradesh";
            r.chapterSubtitle = "Chapter II: Mist & Lamps on the Ganges (Preview / Future Destination)";
            r.playableState = DestinationPlayableState.PreviewFutureChapter;
            r.targetSceneName = "Varanasi_Ghats_Scene";
            r.normalizedIndiaMapPosition = new Vector2(0.58f, 0.62f);

            r.themeSummary = "Stepped riverfront ghats, wooden rowboats, morning mist over the Ganges, architectural reflections, and evening Ganga Aarti ceremonies.";
            r.regionalIntroductionNarrative =
                "[Preview Chapter Concept] From the humid groves of Malabar, your journey continues north to Kashi. " +
                "At dawn along Assi Ghat and Dashashwamedh Ghat, winter mist hangs over the water as wooden boats glide past sandstone steps and centuries-old Maratha and Rajput riverfront pavilions.";
            r.verifiedCulturalBackground =
                "[Verified Cultural Fact] Varanasi features 84 riverfront ghats—stepped stone embankments leading down to the Ganges River. " +
                "Assi Ghat is renowned for 'Subah-e-Banaras' (sunrise Vedic recitals and classical music), while Dashashwamedh Ghat hosts the evening Ganga Aarti, a synchronized ceremonial offering of tiered brass lamps.",
            r.primaryReferenceUrl = "https://en.wikipedia.org/wiki/Ghats_in_Varanasi";

            r.skyTopColor = new Color(0.24f, 0.34f, 0.48f);
            r.skyHorizonColor = new Color(0.88f, 0.64f, 0.42f);
            r.ambientLightColor = new Color(0.55f, 0.48f, 0.45f);
            r.keyLightColor = new Color(1.0f, 0.78f, 0.52f);
            r.accentUiColor = new Color(0.92f, 0.66f, 0.24f);
            r.fogDensity = 0.025f;
            r.fogColor = new Color(0.75f, 0.68f, 0.62f);

            r.soundscapeProfileId = "varanasi_river_dawn";
            r.ambientSoundscapeDescription = "Planned soundscape: gentle lapping of river water against oars, distant temple bells, morning tanpura drone, and riverside murmur.";
            r.plannedPhotographySubjects = new List<string>
            {
                "Stepped Sandstone Ghats & Heritage Pavilions",
                "Wooden Rowboats in Golden Morning Mist",
                "Shimmering Architectural Reflections on the Ganges",
                "Evening Brass Lamp Ceremonies at Dashashwamedh Ghat",
                "Everyday Riverfront Life & Silk Weavers' Lanes"
            };
            return r;
        }

        private static RegionData CreateRajasthanDesertFestival()
        {
            RegionData r = ScriptableObject.CreateInstance<RegionData>();
            r.name = "Region_03_Rajasthan_DesertFestival";
            r.regionId = "rajasthan_desert";
            r.regionTitle = "Rajasthan — The Desert Festival (Maru Mahotsav)";
            r.stateOrTerritory = "Rajasthan (Jaisalmer & Sam Sand Dunes)";
            r.chapterSubtitle = "Chapter III: Golden Sandstone & Songs of the Thar (Preview / Future Destination)";
            r.playableState = DestinationPlayableState.PreviewFutureChapter;
            r.targetSceneName = "Rajasthan_Desert_Scene";
            r.normalizedIndiaMapPosition = new Vector2(0.22f, 0.66f);

            r.themeSummary = "Wind-sculpted Thar Desert dunes, golden Jaisalmer sandstone architecture, vibrant block-printed textiles, Kalbelia dance, and Manganiyar folk music at sunset.";
            r.regionalIntroductionNarrative =
                "[Preview Chapter Concept] In the arid expanse of the Thar Desert outside Jaisalmer, late-afternoon sunlight turns carved yellow sandstone havelis and sweeping dunes into liquid gold.";
            r.verifiedCulturalBackground =
                "[Verified Cultural Fact] The Jaisalmer Desert Festival (Maru Mahotsav) celebrates the living desert heritage of western Rajasthan. " +
                "Performances feature the hereditary Manganiyar and Langa musician communities playing the kamaicha and khartal alongside the UNESCO-recognized Kalbelia folk song and dance tradition.",
            r.primaryReferenceUrl = "https://jaisalmerdesertfestival.rajasthan.gov.in/";

            r.skyTopColor = new Color(0.18f, 0.36f, 0.68f);
            r.skyHorizonColor = new Color(0.96f, 0.58f, 0.22f);
            r.ambientLightColor = new Color(0.68f, 0.46f, 0.28f);
            r.keyLightColor = new Color(1.0f, 0.72f, 0.35f);
            r.accentUiColor = new Color(0.95f, 0.55f, 0.15f);
            r.fogDensity = 0.008f;
            r.fogColor = new Color(0.85f, 0.62f, 0.38f);

            r.soundscapeProfileId = "rajasthan_desert_wind";
            r.ambientSoundscapeDescription = "Planned soundscape: dry desert wind across sand ridges, rhythmic khartal clappers, and resonant kamaicha string melodies.";
            r.plannedPhotographySubjects = new List<string>
            {
                "Wind-Rippled Sam Sand Dunes at Golden Hour",
                "Carved Jharokha Balconies of Jaisalmer Sandstone",
                "Kalbelia Dancers & Embroidered Mirror-Work Textiles",
                "Manganiyar Folk Musicians & Kamaicha Instruments",
                "Local Block-Printing & Pottery Artisans"
            };
            return r;
        }

        private static RegionData CreateKolkataDurgaPuja()
        {
            RegionData r = ScriptableObject.CreateInstance<RegionData>();
            r.name = "Region_04_Kolkata_DurgaPuja";
            r.regionId = "kolkata_durgapuja";
            r.regionTitle = "Kolkata — Durga Puja Public Art & Illumination";
            r.stateOrTerritory = "West Bengal";
            r.chapterSubtitle = "Chapter IV: City of Clay, Light & Pandals (Preview / Future Destination)";
            r.playableState = DestinationPlayableState.PreviewFutureChapter;
            r.targetSceneName = "Kolkata_DurgaPuja_Scene";
            r.normalizedIndiaMapPosition = new Vector2(0.72f, 0.52f);

            r.themeSummary = "Densely layered urban streets, Kumartuli clay sculpture workshops, elaborate bamboo-and-cloth pandal art installations, Chandannagar LED/bulb illuminations, and Dhak drummers.";
            r.regionalIntroductionNarrative =
                "[Preview Chapter Concept] Every autumn, Kolkata transforms into an open-air public art gallery where narrow north-city lanes with louvered green shutters open onto towering contemporary and traditional pandals.";
            r.verifiedCulturalBackground =
                "[Verified Cultural Fact] Inscribed in 2021 on UNESCO's Representative List of the Intangible Cultural Heritage of Humanity, Durga Puja in Kolkata blends worship with community-funded public art installations (pandals), traditional idol-making in neighborhoods like Kumartuli using river clay, and the rhythmic beat of the Dhak drum.",
            r.primaryReferenceUrl = "https://ich.unesco.org/en/RL/durga-puja-in-kolkata-00703";

            r.skyTopColor = new Color(0.08f, 0.06f, 0.18f);
            r.skyHorizonColor = new Color(0.45f, 0.16f, 0.32f);
            r.ambientLightColor = new Color(0.38f, 0.22f, 0.34f);
            r.keyLightColor = new Color(1.0f, 0.48f, 0.62f);
            r.accentUiColor = new Color(0.92f, 0.28f, 0.48f);
            r.fogDensity = 0.012f;
            r.fogColor = new Color(0.18f, 0.10f, 0.22f);

            r.soundscapeProfileId = "kolkata_festival_streets";
            r.ambientSoundscapeDescription = "Planned soundscape: festive urban street murmur, resonant Dhak percussion, conch shells (Shankha), and tram bells.";
            r.plannedPhotographySubjects = new List<string>
            {
                "Thematic Pandal Art Installations & Bamboo Craft",
                "Kumartuli Artisan Clay Sculpting Details",
                "Illuminated North Kolkata Heritage Streetscapes",
                "Dhak Drummers & Dhunuchi Dance Motion Studies",
                "Street-Level Community Celebrations & Reflections"
            };
            return r;
        }

        private static RegionData CreateNagalandHornbillFestival()
        {
            RegionData r = ScriptableObject.CreateInstance<RegionData>();
            r.name = "Region_05_Nagaland_Hornbill";
            r.regionId = "nagaland_hornbill";
            r.regionTitle = "Nagaland — Hornbill Festival at Kisama";
            r.stateOrTerritory = "Nagaland (Kisama Heritage Village, Kohima)";
            r.chapterSubtitle = "Chapter V: Morungs of the Hills (Preview / Future Destination)";
            r.playableState = DestinationPlayableState.PreviewFutureChapter;
            r.targetSceneName = "Nagaland_Hornbill_Scene";
            r.normalizedIndiaMapPosition = new Vector2(0.88f, 0.64f);

            r.themeSummary = "Misty Naga Hills, distinct tribe-specific Morung architecture, handwoven shawls, log drums, bamboo craft displays, and community folk performances.";
            r.regionalIntroductionNarrative =
                "[Preview Chapter Concept] Set against the forested slopes near Kohima, Kisama Heritage Village brings together distinct Naga communities, each represented by its own traditional Morung (youth dormitory) with unique woodcarvings and textiles.";
            r.verifiedCulturalBackground =
                "[Verified Cultural Fact] Held annually from December 1–10 at Kisama Heritage Village near Kohima, the Hornbill Festival showcases the distinct languages, woven textiles, folk songs, and Morung architectural traditions of Nagaland's 17 recognized Naga tribes and the Kachari community—each maintaining its own specific motifs, shawl patterns, and oral histories.",
            r.primaryReferenceUrl = "https://tourism.nagaland.gov.in/naga-heritage-village/";

            r.skyTopColor = new Color(0.16f, 0.38f, 0.34f);
            r.skyHorizonColor = new Color(0.62f, 0.75f, 0.68f);
            r.ambientLightColor = new Color(0.42f, 0.52f, 0.46f);
            r.keyLightColor = new Color(0.95f, 0.88f, 0.74f);
            r.accentUiColor = new Color(0.28f, 0.76f, 0.56f);
            r.fogDensity = 0.020f;
            r.fogColor = new Color(0.52f, 0.64f, 0.58f);

            r.soundscapeProfileId = "nagaland_mountain_breeze";
            r.ambientSoundscapeDescription = "Planned soundscape: high-hill pine wind, hollowed wooden log drum resonance, and polyphonic choral folk singing.";
            r.plannedPhotographySubjects = new List<string>
            {
                "Tribe-Specific Morung Architecture & Woodcarvings",
                "Carved Wooden Log Drums & Bamboo Craft Displays",
                "Handwoven Naga Shawls & Beadwork Details",
                "Amphitheatre Folk Performances & Choral Groups",
                "Misty Ridges of the Japfu Range"
            };
            return r;
        }

        private static RegionData CreateLadakhMonasticFestivals()
        {
            RegionData r = ScriptableObject.CreateInstance<RegionData>();
            r.name = "Region_06_Ladakh_Monastic";
            r.regionId = "ladakh_hemis";
            r.regionTitle = "Ladakh — High Himalayas & Monastic Festivals";
            r.stateOrTerritory = "Ladakh (Indus Valley & Hemis)";
            r.chapterSubtitle = "Chapter VI: Citadels Above the Clouds (Preview / Future Destination)";
            r.playableState = DestinationPlayableState.PreviewFutureChapter;
            r.targetSceneName = "Ladakh_Monastic_Scene";
            r.normalizedIndiaMapPosition = new Vector2(0.38f, 0.88f);

            r.themeSummary = "High-altitude Trans-Himalayan mountains, whitewashed hilltop gompas, fluttering prayer flags, crisp cobalt skies, and sacred Cham masked dances.";
            r.regionalIntroductionNarrative =
                "[Preview Chapter Concept] At over 3,500 meters in the rain shadow of the Himalayas, ultra-clear mountain light carves sharp shadows across ochre cliffs and whitewashed monastic courtyards.";
            r.verifiedCulturalBackground =
                "[Verified Cultural Fact] The Hemis Tsechu festival at Hemis Monastery (belonging to the Drukpa lineage of Tibetan Buddhism) honors Guru Padmasambhava. Monks perform sacred Cham masked dances in the monastery courtyard accompanied by long telescopic copper horns (Dungchen), cymbals (Rolmo), and drums.",
            r.primaryReferenceUrl = "https://ignca.gov.in/the-hemis-festival/";

            r.skyTopColor = new Color(0.08f, 0.26f, 0.64f);
            r.skyHorizonColor = new Color(0.58f, 0.76f, 0.94f);
            r.ambientLightColor = new Color(0.52f, 0.60f, 0.72f);
            r.keyLightColor = new Color(1.0f, 0.96f, 0.90f);
            r.accentUiColor = new Color(0.32f, 0.68f, 0.96f);
            r.fogDensity = 0.004f;
            r.fogColor = new Color(0.62f, 0.75f, 0.90f);

            r.soundscapeProfileId = "ladakh_high_altitude";
            r.ambientSoundscapeDescription = "Planned soundscape: high-altitude mountain wind, fluttering prayer flags, deep Dungchen long-horn resonance, and courtyard cymbals.";
            r.plannedPhotographySubjects = new List<string>
            {
                "Whitewashed Gompa Architecture on Stark Mountain Cliffs",
                "Sacred Cham Masked Dance Courtyards at Hemis",
                "Wide-Angle Trans-Himalayan Mountain Valleys & Cloud Shadows",
                "Dungchen Long Horns & Silk Brocade Costumes",
                "Chortens (Stupas) & Prayer Flags Against Cobalt Skies"
            };
            return r;
        }
    }
}
