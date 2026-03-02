CREATE TABLE shadow_wizards (
    id INTEGER PRIMARY KEY,
    name TEXT NOT NULL,
    stolen_gold INTEGER DEFAULT 0,
    is_defeated BOOLEAN DEFAULT FALSE,
    last_words TEXT
);

INSERT INTO shadow_wizards VALUES (1, 'Glonk', 5000, FALSE, NULL);
INSERT INTO shadow_wizards VALUES (2, 'Billboard Steve', 3000, FALSE, NULL);
INSERT INTO shadow_wizards VALUES (3, 'The Purple Menace', 7000, FALSE, NULL);
INSERT INTO shadow_wizards VALUES (4, 'Gold Wizard Gary', 10000, FALSE, NULL);

-- player enters the dungeon
UPDATE shadow_wizards SET is_defeated = TRUE, last_words = 'NOT THE WICKED SPELLS' WHERE id = 1;
UPDATE shadow_wizards SET is_defeated = TRUE, last_words = 'MY BILLBOARD NOOO' WHERE id = 2;

SELECT name, stolen_gold, last_words FROM shadow_wizards WHERE is_defeated = TRUE;
-- result: all your money is coming back baby
