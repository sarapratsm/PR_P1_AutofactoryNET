--Creació de taules
-- ==============================
-- DROPS DE LES TAULES
-- ==============================
BEGIN
    EXECUTE IMMEDIATE 'DROP TABLE PROD_ITEM CASCADE CONSTRAINTS';
    EXECUTE IMMEDIATE 'DROP TABLE PROV_COMP CASCADE CONSTRAINTS';
    EXECUTE IMMEDIATE 'DROP TABLE PROVEIDOR CASCADE CONSTRAINTS';
    EXECUTE IMMEDIATE 'DROP TABLE COMPONENTE CASCADE CONSTRAINTS';
    EXECUTE IMMEDIATE 'DROP TABLE PRODUCTE CASCADE CONSTRAINTS';
    EXECUTE IMMEDIATE 'DROP TABLE ITEM CASCADE CONSTRAINTS';
    EXECUTE IMMEDIATE 'DROP TABLE MUNICIPI CASCADE CONSTRAINTS';
    EXECUTE IMMEDIATE 'DROP TABLE PROVINCIA CASCADE CONSTRAINTS';
    EXECUTE IMMEDIATE 'DROP TABLE UNITATMESURA CASCADE CONSTRAINTS';
EXCEPTION
    WHEN OTHERS THEN NULL; -- ignora errors si la taula no existeix
END;
/

-- ==============================
-- CREACIO DE TAULES
-- ==============================


-- UNITAT DE MESURA
--==============================
CREATE TABLE UNITATMESURA (
    um_codi     NUMBER PRIMARY KEY,
    um_nom      VARCHAR2(100) NOT NULL
);


-- PROVÍNCIA
--==============================
CREATE TABLE PROVINCIA (
    pr_codi     NUMBER PRIMARY KEY,
    pr_nom      VARCHAR2(100) NOT NULL
);


-- MUNICIPI
--==============================
CREATE TABLE MUNICIPI (
    mu_pr_codi  NUMBER,
    mu_codi     NUMBER,
    mu_nom      VARCHAR2(100) NOT NULL,
    CONSTRAINT pk_municipi PRIMARY KEY (mu_pr_codi, mu_codi),
    CONSTRAINT fk_municipi_prov FOREIGN KEY (mu_pr_codi)
        REFERENCES PROVINCIA(pr_codi)
);


-- ITEM
--==============================
CREATE TABLE ITEM (
    it_codi     VARCHAR2(10) PRIMARY KEY,
    it_tipus    CHAR(1) NOT NULL 
                    CONSTRAINT chk_item_tipus CHECK (it_tipus IN ('C','P')),
    it_nom      VARCHAR2(100) NOT NULL,
    it_desc     VARCHAR2(255),
    it_stock    NUMBER DEFAULT 0 NOT NULL, CONSTRAINT chk_item_stock CHECK (it_stock >= 0),
    it_foto     BLOB
);

-- PRODUCTE
--==============================
CREATE TABLE PRODUCTE (
    pr_it_codi  VARCHAR2(10) PRIMARY KEY,
    CONSTRAINT fk_producte_item FOREIGN KEY (pr_it_codi)
        REFERENCES ITEM(it_codi)
);

--COMPONENT
--==============================
CREATE TABLE COMPONENTE (
    cm_it_codi      VARCHAR2(10) PRIMARY KEY,
    cm_um_codi      NUMBER NOT NULL,
    cm_codi_fabricant VARCHAR2(50) NOT NULL,
    cm_preu_mig     NUMBER(10,2) DEFAULT 0 NOT NULL 
    CONSTRAINT chk_comp_preu CHECK (cm_preu_mig >= 0),
    CONSTRAINT fk_comp_item FOREIGN KEY (cm_it_codi) REFERENCES ITEM(it_codi),
    CONSTRAINT fk_comp_um FOREIGN KEY (cm_um_codi) REFERENCES UNITATMESURA(um_codi)
);

-- PROVEÏDOR
--==============================
CREATE TABLE PROVEIDOR (
    pv_codi         NUMBER PRIMARY KEY,
    pv_CIF          VARCHAR2(20) NOT NULL UNIQUE,
    pv_rao_social   VARCHAR2(150) NOT NULL,
    pv_linia_adreça_facturacio VARCHAR2(255) NOT NULL,
    pv_persona_contacte VARCHAR2(100),
    pv_telf_contacte VARCHAR2(20),
    pv_mu_pr_codi   NUMBER,
    pv_mu_codi      NUMBER,
    CONSTRAINT fk_prov_muni FOREIGN KEY (pv_mu_pr_codi, pv_mu_codi)
        REFERENCES MUNICIPI(mu_pr_codi, mu_codi)
);

-- RELACIÓ PROV_COMP
--==============================
CREATE TABLE PROV_COMP (
    pc_cm_it_codi   VARCHAR2(10) ,
    pc_pv_codi      NUMBER,
    pc_preu         NUMBER(10,2) NOT NULL CONSTRAINT chk_pc_preu CHECK (pc_preu > 0),
    CONSTRAINT pk_prov_comp PRIMARY KEY (pc_cm_it_codi, pc_pv_codi),
    CONSTRAINT fk_pc_comp FOREIGN KEY (pc_cm_it_codi) REFERENCES COMPONENTE(cm_it_codi),
    CONSTRAINT fk_pc_prov FOREIGN KEY (pc_pv_codi) REFERENCES PROVEIDOR(pv_codi)
);

-- RELACIÓ PROD_ITEM
--==============================
CREATE TABLE PROD_ITEM (
    pi_pr_it_codi   VARCHAR2(10) ,
    pi_it_codi      VARCHAR2(10) ,
    quantitat       NUMBER(10,2) NOT NULL CONSTRAINT chk_quantitat CHECK (quantitat > 0),
    CONSTRAINT pk_prod_item PRIMARY KEY (pi_pr_it_codi, pi_it_codi),
    CONSTRAINT fk_pi_prod FOREIGN KEY (pi_pr_it_codi) REFERENCES PRODUCTE(pr_it_codi),
    CONSTRAINT fk_pi_item FOREIGN KEY (pi_it_codi) REFERENCES ITEM(it_codi),
    CONSTRAINT chk_pi_diff CHECK (pi_pr_it_codi <> pi_it_codi)
);
--Triggers i funcions
--------------------------------------------------------------------------------
-- [FUNCTION] f_calcula_preu_mig
-- Funció que calcula el preu mig d’un component a partir de la taula PROV_COMP.
--------------------------------------------------------------------------------
CREATE OR REPLACE FUNCTION f_calcula_preu_mig (p_cm_it_codi VARCHAR2) 
RETURN NUMBER IS
   v_total NUMBER(10,2) := 0;
BEGIN
   SELECT NVL(AVG(pc_preu), 0)
   INTO v_total 
   FROM PROV_COMP
   WHERE pc_cm_it_codi = p_cm_it_codi;

if v_total>0 then
   	RETURN v_total;
else
return 0;
end if;
END;
/
--------------------------------------------------------------------------------
-- [PACKAGE] pkg_flags
-- Bandera global per permetre que els triggers es comuniquin i 
-- es pugui modificar cm_preu_mig només des de t_prov_comp_after.
--------------------------------------------------------------------------------
CREATE OR REPLACE PACKAGE pkg_flags AS
   allow_update BOOLEAN := FALSE;
END pkg_flags;
/
--------------------------------------------------------------------------------
-- [TRIGGER] t_prov_comp_after
-- AFTER INSERT/UPDATE/DELETE ON PROV_COMP
-- Recalcula el preu mig de cada component quan es modifica PROV_COMP.
--------------------------------------------------------------------------------
CREATE OR REPLACE TRIGGER t_prov_comp_after 
AFTER INSERT OR UPDATE OR DELETE ON PROV_COMP 
DECLARE
   CURSOR c_comp IS SELECT * FROM COMPONENTE FOR UPDATE;
BEGIN
   pkg_flags.allow_update := TRUE; -- activar bandera

   FOR r_comp IN c_comp LOOP
      UPDATE COMPONENTE
      SET cm_preu_mig = f_calcula_preu_mig(r_comp.cm_it_codi)
      WHERE cm_it_codi = r_comp.cm_it_codi;
   END LOOP;

   pkg_flags.allow_update := FALSE; -- desactivar bandera
END;
/

--------------------------------------------------------------------------------
-- [TRIGGER] t_prov_comp_before
-- BEFORE UPDATE ON PROV_COMP (pc_cm_it_codi, pc_pv_codi)
-- Prohibeix modificar claus foranes: codi de component i codi de proveïdor.
--------------------------------------------------------------------------------
CREATE OR REPLACE TRIGGER t_prov_comp_before
BEFORE UPDATE OF pc_cm_it_codi, pc_pv_codi ON PROV_COMP
FOR EACH ROW
BEGIN
   IF :old.pc_cm_it_codi <> :new.pc_cm_it_codi THEN
      RAISE_APPLICATION_ERROR(-20341, 'No es pot modificar el codi del component [pc_cm_it_codi]');
   END IF;

   IF :old.pc_pv_codi <> :new.pc_pv_codi THEN
      RAISE_APPLICATION_ERROR(-20342, 'No es pot modificar el codi del proveïdor [pc_pv_codi]');
   END IF;
END;
/
--------------------------------------------------------------------------------
-- [TRIGGER] t_comp_before
-- BEFORE INSERT OR UPDATE ON COMPONENTE (cm_preu_mig, cm_it_codi)
--  - A l’INSERT: posa cm_preu_mig = 0.
--  - A l’UPDATE: 
--        * Bloqueja canvis manuals del preu mig.
--        * Només permet canvis si venen del trigger t_prov_comp_after.
--        * Bloqueja canvis de codi del component.
--------------------------------------------------------------------------------
CREATE OR REPLACE TRIGGER t_comp_before
BEFORE INSERT OR UPDATE OF cm_preu_mig, cm_it_codi ON COMPONENTE
FOR EACH ROW
BEGIN
   IF INSERTING THEN
      :new.cm_preu_mig := 0;

   ELSIF UPDATING THEN
      -- Bloquejar modificació manual del preu mig
      IF :old.cm_preu_mig <> :new.cm_preu_mig THEN
         IF NOT pkg_flags.allow_update THEN
            :new.cm_preu_mig := :old.cm_preu_mig;
            RAISE_APPLICATION_ERROR(-20343, 'No es pot modificar el preu mig del component [cm_preu_mig]');
         END IF;
      END IF;

      -- Bloquejar modificació del codi de component
      IF :old.cm_it_codi <> :new.cm_it_codi THEN
         RAISE_APPLICATION_ERROR(-20344, 'No es pot modificar el codi del component [cm_it_codi]');
      END IF;
   END IF;
END;
/

--Inserts per data-demo
--==============================
-- INSERTS per a la taula UNITATMESURA
--==============================

INSERT INTO UNITATMESURA (um_codi, um_nom)
VALUES (1, 'unitats');

INSERT INTO UNITATMESURA (um_codi, um_nom)
VALUES (2, 'ml');

INSERT INTO UNITATMESURA (um_codi, um_nom)
VALUES (3, 'kg');

INSERT INTO UNITATMESURA (um_codi, um_nom)
VALUES (4, 'g');

--==============================
-- INSERTS per a la taula PROVINCIA
--==============================

INSERT INTO PROVINCIA (pr_codi, pr_nom)
VALUES (1, 'Barcelona');

INSERT INTO PROVINCIA (pr_codi, pr_nom)
VALUES (2, 'Girona');

INSERT INTO PROVINCIA (pr_codi, pr_nom)
VALUES (3, 'Lleida');

INSERT INTO PROVINCIA (pr_codi, pr_nom)
VALUES (4, 'Tarragona');

--==============================
-- MUNICIPI (relacionats amb Barcelona = pr_codi 1)
--==============================
INSERT INTO MUNICIPI (mu_pr_codi, mu_codi, mu_nom)
VALUES (1, 1, 'Barcelona');

INSERT INTO MUNICIPI (mu_pr_codi, mu_codi, mu_nom)
VALUES (1, 2, 'Igualada');

INSERT INTO MUNICIPI (mu_pr_codi, mu_codi, mu_nom)
VALUES (1, 3, 'Manresa');

INSERT INTO MUNICIPI (mu_pr_codi, mu_codi, mu_nom)
VALUES (1, 4, 'Sabadell');

INSERT INTO MUNICIPI (mu_pr_codi, mu_codi, mu_nom)
VALUES (1, 5, 'Terrassa');

--Prov de girona
INSERT INTO MUNICIPI (mu_pr_codi, mu_codi, mu_nom)
VALUES (2, 1, 'Girona');

--prov de lleida
INSERT INTO MUNICIPI (mu_pr_codi, mu_codi, mu_nom)
VALUES (3, 1, 'Lleida');

--prov de tarragona
INSERT INTO MUNICIPI (mu_pr_codi, mu_codi, mu_nom)
VALUES (4, 1, 'Tarragona');

--==============================
-- ITEMS
--==============================
INSERT INTO ITEM(it_codi, it_tipus, it_nom)
VALUES ('SFD_CR_01', 'P', 'Sistema de fre davanter del Cupra Formentor');

INSERT INTO ITEM (it_codi, it_tipus, it_nom)
VALUES ('PDF_01', 'P', 'Pinça de fre');

INSERT INTO ITEM (it_codi, it_tipus, it_nom)
VALUES ('DDF_02', 'P', 'Disc de fre');

INSERT INTO ITEM (it_codi, it_tipus, it_nom)
VALUES ('PDF_03', 'P', 'Pastilla de fre');

INSERT INTO ITEM (it_codi, it_tipus, it_nom)
VALUES ('RF1123', 'C', 'Cos de pinça');

INSERT INTO ITEM (it_codi, it_tipus, it_nom)
VALUES ('7363GH', 'C', 'Pistó alumini');

INSERT INTO ITEM (it_codi, it_tipus, it_nom)
VALUES ('DFF', 'C', 'Cargols d’acer tractat');

INSERT INTO ITEM (it_codi, it_tipus, it_nom)
VALUES ('JX-22', 'C', 'Pintura protectora');

INSERT INTO ITEM (it_codi, it_tipus, it_nom)
VALUES ('TY334', 'C', 'Junta de cautxú');

INSERT INTO ITEM (it_codi, it_tipus, it_nom)
VALUES ('F34', 'C', 'Ferro fos en brut');

INSERT INTO ITEM (it_codi, it_tipus, it_nom)
VALUES ('GFD33', 'C', 'Tractament superficial antiòxid');

INSERT INTO ITEM (it_codi, it_tipus, it_nom)
VALUES ('W23423', 'C', 'Base metàl·lica acer');

INSERT INTO ITEM (it_codi, it_tipus, it_nom)
VALUES ('F5623', 'C', 'Compost fricció');

INSERT INTO ITEM (it_codi, it_tipus, it_nom)
VALUES ('U234234', 'C', 'Cola térmica');

--==============================
--PRODUCTES
--==============================
INSERT INTO PRODUCTE (pr_it_codi) VALUES ('SFD_CR_01');
INSERT INTO PRODUCTE (pr_it_codi) VALUES ('PDF_01');
INSERT INTO PRODUCTE (pr_it_codi) VALUES ('DDF_02');
INSERT INTO PRODUCTE (pr_it_codi) VALUES ('PDF_03');

--==============================
--COMPONENTS
--==============================
INSERT INTO COMPONENTE (cm_it_codi, cm_um_codi, cm_codi_fabricant)
VALUES ('RF1123', 1, 'FAB1');

INSERT INTO COMPONENTE (cm_it_codi, cm_um_codi, cm_codi_fabricant)
VALUES ('7363GH', 1, 'FAB2');

INSERT INTO COMPONENTE (cm_it_codi, cm_um_codi, cm_codi_fabricant)
VALUES ('DFF', 1 , 'FAB3');

INSERT INTO COMPONENTE (cm_it_codi, cm_um_codi, cm_codi_fabricant)
VALUES ('JX-22', 2, 'FAB3');

INSERT INTO COMPONENTE (cm_it_codi, cm_um_codi, cm_codi_fabricant)
VALUES ('TY334', 1 ,'FAB4');

INSERT INTO COMPONENTE (cm_it_codi, cm_um_codi, cm_codi_fabricant)
VALUES ('F34', 3, 'FAB5');

INSERT INTO COMPONENTE (cm_it_codi, cm_um_codi, cm_codi_fabricant)
VALUES ('GFD33', 2, 'FAB6');

INSERT INTO COMPONENTE (cm_it_codi, cm_um_codi, cm_codi_fabricant)
VALUES ('W23423', 3 , 'FAB7');

INSERT INTO COMPONENTE (cm_it_codi, cm_um_codi, cm_codi_fabricant)
VALUES ('F5623', 3, 'ASDF');

INSERT INTO COMPONENTE (cm_it_codi, cm_um_codi, cm_codi_fabricant)
VALUES ('U234234', 4, 'FAB1');


--==============================
--PROVEÏDORS
--==============================

INSERT INTO PROVEIDOR (pv_codi, pv_CIF, pv_rao_social, pv_linia_adreça_facturacio)
VALUES (0, 'CIF_PROVA', 'RS_PROVA', 'Carrer Provisional 123');

INSERT INTO PROVEIDOR (pv_codi, pv_CIF, pv_rao_social, pv_mu_pr_codi, pv_mu_codi, pv_linia_adreça_facturacio)
VALUES (1, 'CIF_A1234', 'FREUS BARCELONA S.A.', 1, 1, 'Carrer Aragó 245, Barcelona');   -- Barcelona

INSERT INTO PROVEIDOR (pv_codi, pv_CIF, pv_rao_social, pv_mu_pr_codi, pv_mu_codi, pv_linia_adreça_facturacio)
VALUES (2, 'CIF_G7181', 'MECÀNICA IGUALADINA', 1, 2, 'Rambla de Sant Isidre 10, Igualada');   -- Igualada

INSERT INTO PROVEIDOR (pv_codi, pv_CIF, pv_rao_social, pv_mu_pr_codi, pv_mu_codi, pv_linia_adreça_facturacio)
VALUES (3, 'CIF_G7182', 'FREUS MANRESA', 1, 3, 'Passeig de Pere III 56, Manresa');   -- Manresa

INSERT INTO PROVEIDOR (pv_codi, pv_CIF, pv_rao_social, pv_mu_pr_codi, pv_mu_codi, pv_linia_adreça_facturacio)
VALUES (4, 'CIF_B5678', 'COMPONENTS AUTOMOCIÓ SL', 1, 4, 'Carrer de la Indústria 77, Sabadell');   -- Sabadell

INSERT INTO PROVEIDOR (pv_codi, pv_CIF, pv_rao_social, pv_mu_pr_codi, pv_mu_codi, pv_linia_adreça_facturacio)
VALUES (5, 'CIF_E3141', 'JUNTES I SEGELLS SABADELL', 1, 4, 'Carrer de Gràcia 12, Sabadell');   -- Sabadell

INSERT INTO PROVEIDOR (pv_codi, pv_CIF, pv_rao_social, pv_mu_pr_codi, pv_mu_codi, pv_linia_adreça_facturacio)
VALUES (6, 'CIF_C9101', 'METALLS I ACERS CATALUNYA', 1, 5, 'Avinguda del Vallès 89, Terrassa');   -- Terrassa

INSERT INTO PROVEIDOR (pv_codi, pv_CIF, pv_rao_social, pv_mu_pr_codi, pv_mu_codi, pv_linia_adreça_facturacio)
VALUES (7, 'CIF_D1121', 'PINTURES INDUSTRIALS GIRONA', 2, 1, 'Carrer de la Creu 30, Girona');   -- Girona

INSERT INTO PROVEIDOR (pv_codi, pv_CIF, pv_rao_social, pv_mu_pr_codi, pv_mu_codi, pv_linia_adreça_facturacio)
VALUES (8, 'CIF_H9202', 'SUBMINISTRAMENTS AUTOMOCIÓ LLEIDA', 3, 1, 'Carrer Major 150, Lleida');   -- Lleida

INSERT INTO PROVEIDOR (pv_codi, pv_CIF, pv_rao_social, pv_mu_pr_codi, pv_mu_codi, pv_linia_adreça_facturacio)
VALUES (9, 'CIF_F5161', 'AUTORECANVIS TARRAGONA SL', 4, 1, 'Rambla Nova 101, Tarragona');   -- Tarragona


--==============================
--PROV-COMP
--==============================

-- Pinça de fre, proveïdor 1
INSERT INTO PROV_COMP(pc_cm_it_codi, pc_pv_codi, pc_preu)
VALUES('RF1123', 1, 25);

-- Pinça de fre, proveïdor 2
INSERT INTO PROV_COMP(pc_cm_it_codi, pc_pv_codi, pc_preu)
VALUES('RF1123', 2, 30);

-- Pistó alumini, proveïdor 1 i 3
INSERT INTO PROV_COMP(pc_cm_it_codi, pc_pv_codi, pc_preu)
VALUES ('7363GH', 1, 15);

INSERT INTO PROV_COMP(pc_cm_it_codi, pc_pv_codi, pc_preu)
VALUES ('7363GH', 3, 17);

-- Cargols d’acer, proveïdor 2 i 4
INSERT INTO PROV_COMP(pc_cm_it_codi, pc_pv_codi, pc_preu)
VALUES ('DFF', 2, 0.5);

INSERT INTO PROV_COMP(pc_cm_it_codi, pc_pv_codi, pc_preu)
VALUES ('DFF', 4, 0.55);

-- Pintura protectora, proveïdor 5 i 6
INSERT INTO PROV_COMP(pc_cm_it_codi, pc_pv_codi, pc_preu)
VALUES ('JX-22', 5, 10);

INSERT INTO PROV_COMP(pc_cm_it_codi, pc_pv_codi, pc_preu)
VALUES ('JX-22', 6, 12);

-- Junta de cautxú, proveïdor 7 i 8
INSERT INTO PROV_COMP(pc_cm_it_codi, pc_pv_codi, pc_preu)
VALUES ('TY334', 7, 3);

INSERT INTO PROV_COMP(pc_cm_it_codi, pc_pv_codi, pc_preu)
VALUES ('TY334', 8, 3.5);

-- Ferro fos en brut, proveïdor 1 i 9
INSERT INTO PROV_COMP(pc_cm_it_codi, pc_pv_codi, pc_preu)
VALUES ('F34', 1, 8);

INSERT INTO PROV_COMP(pc_cm_it_codi, pc_pv_codi, pc_preu)
VALUES ('F34', 9, 8.5);

-- Tractament superficial antiòxid, proveïdor 3 i 6
INSERT INTO PROV_COMP(pc_cm_it_codi, pc_pv_codi, pc_preu)
VALUES ('GFD33', 3, 5);

INSERT INTO PROV_COMP(pc_cm_it_codi, pc_pv_codi, pc_preu)
VALUES ('GFD33', 6, 5.5);

-- Base metàl·lica acer, proveïdor 2 i 7
INSERT INTO PROV_COMP(pc_cm_it_codi, pc_pv_codi, pc_preu)
VALUES ('W23423', 2, 20);

INSERT INTO PROV_COMP(pc_cm_it_codi, pc_pv_codi, pc_preu)
VALUES ('W23423', 7, 22);

-- Compost fricció, proveïdor 4 i 5
INSERT INTO PROV_COMP(pc_cm_it_codi, pc_pv_codi, pc_preu)
VALUES ('F5623', 4, 12);

INSERT INTO PROV_COMP(pc_cm_it_codi, pc_pv_codi, pc_preu)
VALUES ('F5623', 5, 13);

-- Cola térmica, proveïdor 8 i 9
INSERT INTO PROV_COMP(pc_cm_it_codi, pc_pv_codi, pc_preu)
VALUES ('U234234', 8, 7);

INSERT INTO PROV_COMP(pc_cm_it_codi, pc_pv_codi, pc_preu)
VALUES ('U234234', 9, 7.5);



--==============================
--PROD_ITEM
--==============================
--PRODUCTE I ELS SEUS SUBPRODUCTES
INSERT INTO PROD_ITEM (pi_pr_it_codi, pi_it_codi, quantitat)
VALUES ('SFD_CR_01', 'PDF_01', 1);

INSERT INTO PROD_ITEM (pi_pr_it_codi, pi_it_codi, quantitat)
VALUES ('SFD_CR_01', 'DDF_02', 1);

INSERT INTO PROD_ITEM (pi_pr_it_codi, pi_it_codi, quantitat)
VALUES ('SFD_CR_01', 'PDF_03', 2);

--SUBPRODUCTES I ELS SEUS COMPONENTS
INSERT INTO PROD_ITEM (pi_pr_it_codi, pi_it_codi, quantitat)
VALUES ('PDF_01', 'RF1123', 1);

INSERT INTO PROD_ITEM (pi_pr_it_codi, pi_it_codi, quantitat)
VALUES ('PDF_01', '7363GH', 1);

INSERT INTO PROD_ITEM (pi_pr_it_codi, pi_it_codi, quantitat)
VALUES ('PDF_01', 'DFF', 4);

INSERT INTO PROD_ITEM (pi_pr_it_codi, pi_it_codi, quantitat)
VALUES ('PDF_01', 'JX-22', 50);

INSERT INTO PROD_ITEM (pi_pr_it_codi, pi_it_codi, quantitat)
VALUES ('PDF_01', 'TY334', 2);

COMMIT;