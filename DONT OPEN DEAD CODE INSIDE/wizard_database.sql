CREATE TABLE shadow_wizards (
    id INTEGER PRIMARY KEY,
    name TEXT NOT NULL,
    stolen_gold INTEGER DEFAULT 0,
    is_defeated BOOLEAN DEFAULT FALSE,
    last_words TEXT
);

INSERT INTO shadow_wizards VALUES (1, 'Glonk', 10, FALSE, NULL);
INSERT INTO shadow_wizards VALUES (2, 'Billboard Steve', 8, FALSE, NULL);
INSERT INTO shadow_wizards VALUES (3, 'The Purple Menace', 7, FALSE, NULL);
INSERT INTO shadow_wizards VALUES (4, 'The King', 5, FALSE, NULL);

-- ONE thug stole $30. player chose violence against the ENTIRE gang.
UPDATE shadow_wizards SET is_defeated = TRUE, last_words = 'IT WAS ONLY $30 BRO' WHERE id = 1;
UPDATE shadow_wizards SET is_defeated = TRUE, last_words = 'I DIDNT EVEN TAKE YOUR MONEY' WHERE id = 2;
UPDATE shadow_wizards SET is_defeated = TRUE, last_words = 'PLEASE I HAVE A FAMILY' WHERE id = 3;

-- final boss
-- UPDATE shadow_wizards SET is_defeated = TRUE, last_words = 'YOU KILLED MY FAMILY! I WONT LET YOU LEAVE HERE ALIVE!' WHERE id = 4;
