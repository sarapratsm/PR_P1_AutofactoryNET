--------------------------------------------------------
--  SCRIPT BBDD AUTOFACTORY
--------------------------------------------------------

-- 1. ELIMINACIÓN DE TABLAS ANTERIORES (Limpieza)
BEGIN
   FOR cur_rec IN (SELECT table_name FROM user_tables) LOOP
      EXECUTE IMMEDIATE 'DROP TABLE ' || cur_rec.table_name || ' CASCADE CONSTRAINTS';
   END LOOP;
   
   -- Borrar secuencias o paquetes si existen
   BEGIN EXECUTE IMMEDIATE 'DROP PACKAGE PKG_FLAGS'; EXCEPTION WHEN OTHERS THEN NULL; END;
   BEGIN EXECUTE IMMEDIATE 'DROP FUNCTION F_CALCULA_PREU_MIG'; EXCEPTION WHEN OTHERS THEN NULL; END;
END;
/

--------------------------------------------------------
--  2. CREACIÓN DE TABLAS (DDL)
--------------------------------------------------------

CREATE TABLE MUNICIPI (
    MU_PR_CODI NUMBER NOT NULL, 
    MU_CODI NUMBER NOT NULL, 
    MU_NOM VARCHAR2(100) NOT NULL,
    CONSTRAINT PK_MUNICIPI PRIMARY KEY (MU_PR_CODI, MU_CODI)
);

CREATE TABLE PROVINCIA (
    PR_CODI NUMBER NOT NULL, 
    PR_NOM VARCHAR2(100) NOT NULL,
    CONSTRAINT PK_PROVINCIA PRIMARY KEY (PR_CODI)
);

CREATE TABLE UNITATMESURA (
    UM_CODI NUMBER NOT NULL, 
    UM_NOM VARCHAR2(100) NOT NULL,
    CONSTRAINT PK_UNITATMESURA PRIMARY KEY (UM_CODI)
);

CREATE TABLE ITEM (
    IT_CODI VARCHAR2(10) NOT NULL, 
    IT_TIPUS CHAR(1) NOT NULL, 
    IT_NOM VARCHAR2(100) NOT NULL, 
    IT_DESC VARCHAR2(255), 
    IT_STOCK NUMBER DEFAULT 0 NOT NULL, 
    IT_FOTO BLOB,
    CONSTRAINT PK_ITEM PRIMARY KEY (IT_CODI),
    CONSTRAINT CHK_ITEM_TIPUS CHECK (it_tipus IN ('C','P')),
    CONSTRAINT CHK_ITEM_STOCK CHECK (it_stock >= 0)
);

CREATE TABLE COMPONENTE (
    CM_IT_CODI VARCHAR2(10) NOT NULL, 
    CM_UM_CODI NUMBER NOT NULL, 
    CM_CODI_FABRICANT VARCHAR2(50) NOT NULL, 
    CM_PREU_MIG NUMBER(10,2) DEFAULT 0 NOT NULL,
    CONSTRAINT PK_COMPONENTE PRIMARY KEY (CM_IT_CODI),
    CONSTRAINT CHK_COMP_PREU CHECK (cm_preu_mig >= 0)
);

CREATE TABLE PRODUCTE (
    PR_IT_CODI VARCHAR2(10) NOT NULL,
    CONSTRAINT PK_PRODUCTE PRIMARY KEY (PR_IT_CODI)
);

CREATE TABLE PROVEIDOR (
    PV_CODI NUMBER NOT NULL, 
    PV_CIF VARCHAR2(20) NOT NULL, 
    PV_RAO_SOCIAL VARCHAR2(150) NOT NULL, 
    PV_LINIA_ADREÇA_FACTURACIO VARCHAR2(255) NOT NULL, 
    PV_PERSONA_CONTACTE VARCHAR2(100), 
    PV_TELF_CONTACTE VARCHAR2(20), 
    PV_MU_PR_CODI NUMBER, 
    PV_MU_CODI NUMBER,
    CONSTRAINT PK_PROVEIDOR PRIMARY KEY (PV_CODI),
    CONSTRAINT UK_PROVEIDOR_CIF UNIQUE (PV_CIF)
);

CREATE TABLE PROD_ITEM (
    PI_PR_IT_CODI VARCHAR2(10) NOT NULL, 
    PI_IT_CODI VARCHAR2(10) NOT NULL, 
    QUANTITAT NUMBER(10,2) NOT NULL,
    CONSTRAINT PK_PROD_ITEM PRIMARY KEY (PI_PR_IT_CODI, PI_IT_CODI),
    CONSTRAINT CHK_QUANTITAT CHECK (quantitat > 0),
    CONSTRAINT CHK_PI_DIFF CHECK (pi_pr_it_codi <> pi_it_codi)
);

CREATE TABLE PROV_COMP (
    PC_CM_IT_CODI VARCHAR2(10) NOT NULL, 
    PC_PV_CODI NUMBER NOT NULL, 
    PC_PREU NUMBER(10,2) NOT NULL,
    CONSTRAINT PK_PROV_COMP PRIMARY KEY (PC_CM_IT_CODI, PC_PV_CODI),
    CONSTRAINT CHK_PC_PREU CHECK (pc_preu > 0)
);

CREATE TABLE USUARIS (
    US_USERNAME VARCHAR2(50) NOT NULL, 
    US_PASSWORD_HASH VARCHAR2(255) NOT NULL, 
    US_NOM_COMPLET VARCHAR2(100),
    CONSTRAINT PK_USUARIS PRIMARY KEY (US_USERNAME)
);

--------------------------------------------------------
--  3. RELACIONES (Foreign Keys)
--------------------------------------------------------

-- Municipis i Provincies
ALTER TABLE MUNICIPI ADD CONSTRAINT FK_MUNICIPI_PROV FOREIGN KEY (MU_PR_CODI) REFERENCES PROVINCIA (PR_CODI);

-- Proveidor amb Municipi
ALTER TABLE PROVEIDOR ADD CONSTRAINT FK_PROV_MUNI FOREIGN KEY (PV_MU_PR_CODI, PV_MU_CODI) REFERENCES MUNICIPI (MU_PR_CODI, MU_CODI);

-- Herencia ITEM
ALTER TABLE COMPONENTE ADD CONSTRAINT FK_COMP_ITEM FOREIGN KEY (CM_IT_CODI) REFERENCES ITEM (IT_CODI) ON DELETE CASCADE;
ALTER TABLE PRODUCTE ADD CONSTRAINT FK_PRODUCTE_ITEM FOREIGN KEY (PR_IT_CODI) REFERENCES ITEM (IT_CODI) ON DELETE CASCADE;

-- Componente amb Unitat
ALTER TABLE COMPONENTE ADD CONSTRAINT FK_COMP_UM FOREIGN KEY (CM_UM_CODI) REFERENCES UNITATMESURA (UM_CODI);

-- BOM (Prod_Item)
ALTER TABLE PROD_ITEM ADD CONSTRAINT FK_PI_PROD FOREIGN KEY (PI_PR_IT_CODI) REFERENCES PRODUCTE (PR_IT_CODI);
ALTER TABLE PROD_ITEM ADD CONSTRAINT FK_PI_ITEM FOREIGN KEY (PI_IT_CODI) REFERENCES ITEM (IT_CODI);

-- Prov_Comp
ALTER TABLE PROV_COMP ADD CONSTRAINT FK_PC_COMP FOREIGN KEY (PC_CM_IT_CODI) REFERENCES COMPONENTE (CM_IT_CODI);
ALTER TABLE PROV_COMP ADD CONSTRAINT FK_PC_PROV FOREIGN KEY (PC_PV_CODI) REFERENCES PROVEIDOR (PV_CODI);

--------------------------------------------------------
--  4. PL/SQL: PAQUETES, FUNCIONES Y TRIGGERS
--------------------------------------------------------

-- Package Flags (Evitar taula mutant)
CREATE OR REPLACE PACKAGE PKG_FLAGS AS
   allow_update BOOLEAN := FALSE;
END pkg_flags;
/

-- Función Calcula Preu Mig
CREATE OR REPLACE FUNCTION F_CALCULA_PREU_MIG (p_cm_it_codi VARCHAR2) 
RETURN NUMBER IS
   v_total NUMBER(10,2) := 0;
BEGIN
   SELECT NVL(AVG(pc_preu), 0)
   INTO v_total 
   FROM PROV_COMP
   WHERE pc_cm_it_codi = p_cm_it_codi;

   IF v_total > 0 THEN
   	  RETURN v_total;
   ELSE
      RETURN 0;
   END IF;
END;
/

-- Trigger T_COMP_BEFORE
CREATE OR REPLACE TRIGGER T_COMP_BEFORE 
BEFORE INSERT OR UPDATE OF cm_preu_mig, cm_it_codi ON COMPONENTE
FOR EACH ROW
BEGIN
   IF INSERTING THEN
      :new.cm_preu_mig := 0;

   ELSIF UPDATING THEN
      IF :old.cm_preu_mig <> :new.cm_preu_mig THEN
         IF NOT pkg_flags.allow_update THEN
            :new.cm_preu_mig := :old.cm_preu_mig;
            RAISE_APPLICATION_ERROR(-20343, 'No es pot modificar el preu mig manualment.');
         END IF;
      END IF;

      IF :old.cm_it_codi <> :new.cm_it_codi THEN
         RAISE_APPLICATION_ERROR(-20344, 'No es pot modificar el codi del component.');
      END IF;
   END IF;
END;
/

-- Trigger T_PROV_COMP_BEFORE
CREATE OR REPLACE TRIGGER T_PROV_COMP_BEFORE 
BEFORE UPDATE OF pc_cm_it_codi, pc_pv_codi ON PROV_COMP
FOR EACH ROW
BEGIN
   IF :old.pc_cm_it_codi <> :new.pc_cm_it_codi THEN
      RAISE_APPLICATION_ERROR(-20341, 'No es pot modificar el codi del component en la relació.');
   END IF;
   IF :old.pc_pv_codi <> :new.pc_pv_codi THEN
      RAISE_APPLICATION_ERROR(-20342, 'No es pot modificar el codi del proveïdor en la relació.');
   END IF;
END;
/

-- Trigger T_PROV_COMP_AFTER (Actualitza Preu Mig)
CREATE OR REPLACE TRIGGER T_PROV_COMP_AFTER 
AFTER INSERT OR UPDATE OR DELETE ON PROV_COMP 
DECLARE
   CURSOR c_comp IS SELECT * FROM COMPONENTE FOR UPDATE;
BEGIN
   pkg_flags.allow_update := TRUE; 

   FOR r_comp IN c_comp LOOP
      UPDATE COMPONENTE
      SET cm_preu_mig = f_calcula_preu_mig(r_comp.cm_it_codi)
      WHERE cm_it_codi = r_comp.cm_it_codi;
   END LOOP;

   pkg_flags.allow_update := FALSE; 
END;
/

--------------------------------------------------------
--  5. INSERCIÓN DE DATOS (DML)
--------------------------------------------------------
SET DEFINE OFF;

-- Unitats
Insert into UNITATMESURA (UM_CODI,UM_NOM) values ('1','unitats');
Insert into UNITATMESURA (UM_CODI,UM_NOM) values ('2','ml');
Insert into UNITATMESURA (UM_CODI,UM_NOM) values ('3','kg');
Insert into UNITATMESURA (UM_CODI,UM_NOM) values ('4','g');

-- Provincies
Insert into PROVINCIA (PR_CODI,PR_NOM) values ('1','Barcelona');
Insert into PROVINCIA (PR_CODI,PR_NOM) values ('2','Girona');
Insert into PROVINCIA (PR_CODI,PR_NOM) values ('3','Lleida');
Insert into PROVINCIA (PR_CODI,PR_NOM) values ('4','Tarragona');

-- Municipis
Insert into MUNICIPI (MU_PR_CODI,MU_CODI,MU_NOM) values ('1','1','Barcelona');
Insert into MUNICIPI (MU_PR_CODI,MU_CODI,MU_NOM) values ('1','2','Igualada');
Insert into MUNICIPI (MU_PR_CODI,MU_CODI,MU_NOM) values ('1','3','Manresa');
Insert into MUNICIPI (MU_PR_CODI,MU_CODI,MU_NOM) values ('1','4','Sabadell');
Insert into MUNICIPI (MU_PR_CODI,MU_CODI,MU_NOM) values ('1','5','Terrassa');
Insert into MUNICIPI (MU_PR_CODI,MU_CODI,MU_NOM) values ('2','1','Girona');
Insert into MUNICIPI (MU_PR_CODI,MU_CODI,MU_NOM) values ('3','1','Lleida');
Insert into MUNICIPI (MU_PR_CODI,MU_CODI,MU_NOM) values ('4','1','Tarragona');

-- Items (Generics)
Insert into ITEM (IT_CODI,IT_TIPUS,IT_NOM,IT_DESC,IT_STOCK) values ('PDF_01','P','Pinça de fre','Molt necessari per frenar!','57');
Insert into ITEM (IT_CODI,IT_TIPUS,IT_NOM,IT_DESC,IT_STOCK) values ('DDF_02','P','Disc de fre',null,'46');
Insert into ITEM (IT_CODI,IT_TIPUS,IT_NOM,IT_DESC,IT_STOCK) values ('RF1123','C','Cos de pinça',null,'0');
Insert into ITEM (IT_CODI,IT_TIPUS,IT_NOM,IT_DESC,IT_STOCK) values ('7363GH','C','Pistó alumini',null,'0');
Insert into ITEM (IT_CODI,IT_TIPUS,IT_NOM,IT_DESC,IT_STOCK) values ('DFF','C','Cargols d’acer tractat',null,'0');
Insert into ITEM (IT_CODI,IT_TIPUS,IT_NOM,IT_DESC,IT_STOCK) values ('JX-22','C','Pintura protectora',null,'0');
Insert into ITEM (IT_CODI,IT_TIPUS,IT_NOM,IT_DESC,IT_STOCK) values ('TY334','C','Junta de cautxú',null,'0');
Insert into ITEM (IT_CODI,IT_TIPUS,IT_NOM,IT_DESC,IT_STOCK) values ('GFD33','C','Tractament superficial antiòxid',null,'0');
Insert into ITEM (IT_CODI,IT_TIPUS,IT_NOM,IT_DESC,IT_STOCK) values ('W23423','C','Base metàl·lica acer',null,'0');
Insert into ITEM (IT_CODI,IT_TIPUS,IT_NOM,IT_DESC,IT_STOCK) values ('F5623','C','Compost fricció',null,'0');
Insert into ITEM (IT_CODI,IT_TIPUS,IT_NOM,IT_DESC,IT_STOCK) values ('U234234','C','Cola térmica',null,'0');
Insert into ITEM (IT_CODI,IT_TIPUS,IT_NOM,IT_DESC,IT_STOCK) values ('PDF_03','P','Pastilla de fre','Pastilla de fre del sistema de fre del Cupra Formentor.','50');
Insert into ITEM (IT_CODI,IT_TIPUS,IT_NOM,IT_DESC,IT_STOCK) values ('12313','C','Prova',null,'21');
Insert into ITEM (IT_CODI,IT_TIPUS,IT_NOM,IT_DESC,IT_STOCK) values ('SFD_CR_01','P','Sistema de fre davanter del Cupra Formentor','Producte principal d''AutoFactory.','100');
Insert into ITEM (IT_CODI,IT_TIPUS,IT_NOM,IT_DESC,IT_STOCK) values ('Hola','C','Aproveu-me','Siusplau','10');
Insert into ITEM (IT_CODI,IT_TIPUS,IT_NOM,IT_DESC,IT_STOCK) values ('F34','C','Ferro fos en brut',null,'0');
Insert into ITEM (IT_CODI,IT_TIPUS,IT_NOM,IT_DESC,IT_STOCK) values ('Prova','P','Cruasán','Be kind to him','777');

-- Productes
Insert into PRODUCTE (PR_IT_CODI) values ('DDF_02');
Insert into PRODUCTE (PR_IT_CODI) values ('PDF_01');
Insert into PRODUCTE (PR_IT_CODI) values ('PDF_03');
Insert into PRODUCTE (PR_IT_CODI) values ('Prova');
Insert into PRODUCTE (PR_IT_CODI) values ('SFD_CR_01');

-- Components
Insert into COMPONENTE (CM_IT_CODI,CM_UM_CODI,CM_CODI_FABRICANT,CM_PREU_MIG) values ('RF1123','1','FAB1','11');
Insert into COMPONENTE (CM_IT_CODI,CM_UM_CODI,CM_CODI_FABRICANT,CM_PREU_MIG) values ('7363GH','1','FAB2','16');
Insert into COMPONENTE (CM_IT_CODI,CM_UM_CODI,CM_CODI_FABRICANT,CM_PREU_MIG) values ('DFF','1','FAB3','0,53');
Insert into COMPONENTE (CM_IT_CODI,CM_UM_CODI,CM_CODI_FABRICANT,CM_PREU_MIG) values ('JX-22','2','FAB3','11');
Insert into COMPONENTE (CM_IT_CODI,CM_UM_CODI,CM_CODI_FABRICANT,CM_PREU_MIG) values ('TY334','1','FAB4','3,25');
Insert into COMPONENTE (CM_IT_CODI,CM_UM_CODI,CM_CODI_FABRICANT,CM_PREU_MIG) values ('F34','3','FAB5','8,25');
Insert into COMPONENTE (CM_IT_CODI,CM_UM_CODI,CM_CODI_FABRICANT,CM_PREU_MIG) values ('GFD33','2','FAB6','5,25');
Insert into COMPONENTE (CM_IT_CODI,CM_UM_CODI,CM_CODI_FABRICANT,CM_PREU_MIG) values ('W23423','3','FAB7','21');
Insert into COMPONENTE (CM_IT_CODI,CM_UM_CODI,CM_CODI_FABRICANT,CM_PREU_MIG) values ('F5623','3','ASDF','12,5');
Insert into COMPONENTE (CM_IT_CODI,CM_UM_CODI,CM_CODI_FABRICANT,CM_PREU_MIG) values ('U234234','4','FAB1','7,25');
Insert into COMPONENTE (CM_IT_CODI,CM_UM_CODI,CM_CODI_FABRICANT,CM_PREU_MIG) values ('12313','1','f','0');
Insert into COMPONENTE (CM_IT_CODI,CM_UM_CODI,CM_CODI_FABRICANT,CM_PREU_MIG) values ('Hola','4','Gracies','10');

-- Proveidors
Insert into PROVEIDOR (PV_CODI,PV_CIF,PV_RAO_SOCIAL,PV_LINIA_ADREÇA_FACTURACIO,PV_PERSONA_CONTACTE,PV_TELF_CONTACTE,PV_MU_PR_CODI,PV_MU_CODI) values ('0','CIF_PROVA','RS_PROVA','Carrer Provisional 123',null,null,null,null);
Insert into PROVEIDOR (PV_CODI,PV_CIF,PV_RAO_SOCIAL,PV_LINIA_ADREÇA_FACTURACIO,PV_PERSONA_CONTACTE,PV_TELF_CONTACTE,PV_MU_PR_CODI,PV_MU_CODI) values ('1','CIF_A1234','FREUS BARCELONA S.A.','Carrer Aragó 245, Barcelona',null,null,'1','1');
Insert into PROVEIDOR (PV_CODI,PV_CIF,PV_RAO_SOCIAL,PV_LINIA_ADREÇA_FACTURACIO,PV_PERSONA_CONTACTE,PV_TELF_CONTACTE,PV_MU_PR_CODI,PV_MU_CODI) values ('2','CIF_G7181','MECÀNICA IGUALADINA','Rambla de Sant Isidre 10, Igualada',null,null,'1','2');
Insert into PROVEIDOR (PV_CODI,PV_CIF,PV_RAO_SOCIAL,PV_LINIA_ADREÇA_FACTURACIO,PV_PERSONA_CONTACTE,PV_TELF_CONTACTE,PV_MU_PR_CODI,PV_MU_CODI) values ('3','CIF_G7182','FREUS MANRESA','Passeig de Pere III 56, Manresa',null,null,'1','3');
Insert into PROVEIDOR (PV_CODI,PV_CIF,PV_RAO_SOCIAL,PV_LINIA_ADREÇA_FACTURACIO,PV_PERSONA_CONTACTE,PV_TELF_CONTACTE,PV_MU_PR_CODI,PV_MU_CODI) values ('4','CIF_B5678','COMPONENTS AUTOMOCIÓ SL','Carrer de la Indústria 77, Sabadell',null,null,'1','4');
Insert into PROVEIDOR (PV_CODI,PV_CIF,PV_RAO_SOCIAL,PV_LINIA_ADREÇA_FACTURACIO,PV_PERSONA_CONTACTE,PV_TELF_CONTACTE,PV_MU_PR_CODI,PV_MU_CODI) values ('5','CIF_E3141','JUNTES I SEGELLS SABADELL','Carrer de Gràcia 12, Sabadell',null,null,'1','4');
Insert into PROVEIDOR (PV_CODI,PV_CIF,PV_RAO_SOCIAL,PV_LINIA_ADREÇA_FACTURACIO,PV_PERSONA_CONTACTE,PV_TELF_CONTACTE,PV_MU_PR_CODI,PV_MU_CODI) values ('6','CIF_C9101','METALLS I ACERS CATALUNYA','Avinguda del Vallès 89, Terrassa',null,null,'1','5');
Insert into PROVEIDOR (PV_CODI,PV_CIF,PV_RAO_SOCIAL,PV_LINIA_ADREÇA_FACTURACIO,PV_PERSONA_CONTACTE,PV_TELF_CONTACTE,PV_MU_PR_CODI,PV_MU_CODI) values ('7','CIF_D1121','PINTURES INDUSTRIALS GIRONA','Carrer de la Creu 30, Girona',null,null,'2','1');
Insert into PROVEIDOR (PV_CODI,PV_CIF,PV_RAO_SOCIAL,PV_LINIA_ADREÇA_FACTURACIO,PV_PERSONA_CONTACTE,PV_TELF_CONTACTE,PV_MU_PR_CODI,PV_MU_CODI) values ('8','CIF_H9202','SUBMINISTRAMENTS AUTOMOCIÓ LLEIDA','Carrer Major 150, Lleida',null,null,'3','1');
Insert into PROVEIDOR (PV_CODI,PV_CIF,PV_RAO_SOCIAL,PV_LINIA_ADREÇA_FACTURACIO,PV_PERSONA_CONTACTE,PV_TELF_CONTACTE,PV_MU_PR_CODI,PV_MU_CODI) values ('9','CIF_F5161','AUTORECANVIS TARRAGONA SL','Rambla Nova 101, Tarragona',null,null,'4','1');

-- Prov_Comp
Insert into PROV_COMP (PC_CM_IT_CODI,PC_PV_CODI,PC_PREU) values ('RF1123','1','10');
Insert into PROV_COMP (PC_CM_IT_CODI,PC_PV_CODI,PC_PREU) values ('RF1123','4','12');
Insert into PROV_COMP (PC_CM_IT_CODI,PC_PV_CODI,PC_PREU) values ('7363GH','1','15');
Insert into PROV_COMP (PC_CM_IT_CODI,PC_PV_CODI,PC_PREU) values ('7363GH','3','17');
Insert into PROV_COMP (PC_CM_IT_CODI,PC_PV_CODI,PC_PREU) values ('DFF','2','0,5');
Insert into PROV_COMP (PC_CM_IT_CODI,PC_PV_CODI,PC_PREU) values ('DFF','4','0,55');
Insert into PROV_COMP (PC_CM_IT_CODI,PC_PV_CODI,PC_PREU) values ('JX-22','5','10');
Insert into PROV_COMP (PC_CM_IT_CODI,PC_PV_CODI,PC_PREU) values ('JX-22','6','12');
Insert into PROV_COMP (PC_CM_IT_CODI,PC_PV_CODI,PC_PREU) values ('TY334','7','3');
Insert into PROV_COMP (PC_CM_IT_CODI,PC_PV_CODI,PC_PREU) values ('TY334','8','3,5');
Insert into PROV_COMP (PC_CM_IT_CODI,PC_PV_CODI,PC_PREU) values ('F34','1','8');
Insert into PROV_COMP (PC_CM_IT_CODI,PC_PV_CODI,PC_PREU) values ('F34','9','8,5');
Insert into PROV_COMP (PC_CM_IT_CODI,PC_PV_CODI,PC_PREU) values ('GFD33','3','5');
Insert into PROV_COMP (PC_CM_IT_CODI,PC_PV_CODI,PC_PREU) values ('GFD33','6','5,5');
Insert into PROV_COMP (PC_CM_IT_CODI,PC_PV_CODI,PC_PREU) values ('W23423','2','20');
Insert into PROV_COMP (PC_CM_IT_CODI,PC_PV_CODI,PC_PREU) values ('W23423','7','22');
Insert into PROV_COMP (PC_CM_IT_CODI,PC_PV_CODI,PC_PREU) values ('F5623','4','12');
Insert into PROV_COMP (PC_CM_IT_CODI,PC_PV_CODI,PC_PREU) values ('F5623','5','13');
Insert into PROV_COMP (PC_CM_IT_CODI,PC_PV_CODI,PC_PREU) values ('U234234','8','7');
Insert into PROV_COMP (PC_CM_IT_CODI,PC_PV_CODI,PC_PREU) values ('U234234','9','7,5');
Insert into PROV_COMP (PC_CM_IT_CODI,PC_PV_CODI,PC_PREU) values ('Hola','8','10');

-- Prod_Item (BOM)
Insert into PROD_ITEM (PI_PR_IT_CODI,PI_IT_CODI,QUANTITAT) values ('PDF_03','W23423','1,5');
Insert into PROD_ITEM (PI_PR_IT_CODI,PI_IT_CODI,QUANTITAT) values ('PDF_03','F5623','0,43');
Insert into PROD_ITEM (PI_PR_IT_CODI,PI_IT_CODI,QUANTITAT) values ('PDF_03','U234234','5');
Insert into PROD_ITEM (PI_PR_IT_CODI,PI_IT_CODI,QUANTITAT) values ('SFD_CR_01','DDF_02','1');
Insert into PROD_ITEM (PI_PR_IT_CODI,PI_IT_CODI,QUANTITAT) values ('SFD_CR_01','PDF_03','2');
Insert into PROD_ITEM (PI_PR_IT_CODI,PI_IT_CODI,QUANTITAT) values ('SFD_CR_01','PDF_01','1');
Insert into PROD_ITEM (PI_PR_IT_CODI,PI_IT_CODI,QUANTITAT) values ('PDF_01','DFF','4');
Insert into PROD_ITEM (PI_PR_IT_CODI,PI_IT_CODI,QUANTITAT) values ('PDF_01','JX-22','50');
Insert into PROD_ITEM (PI_PR_IT_CODI,PI_IT_CODI,QUANTITAT) values ('PDF_01','RF1123','1');
Insert into PROD_ITEM (PI_PR_IT_CODI,PI_IT_CODI,QUANTITAT) values ('PDF_01','TY334','2');
Insert into PROD_ITEM (PI_PR_IT_CODI,PI_IT_CODI,QUANTITAT) values ('PDF_01','7363GH','1');
Insert into PROD_ITEM (PI_PR_IT_CODI,PI_IT_CODI,QUANTITAT) values ('DDF_02','DFF','2');
Insert into PROD_ITEM (PI_PR_IT_CODI,PI_IT_CODI,QUANTITAT) values ('DDF_02','F34','1,5');
Insert into PROD_ITEM (PI_PR_IT_CODI,PI_IT_CODI,QUANTITAT) values ('DDF_02','JX-22','20');
Insert into PROD_ITEM (PI_PR_IT_CODI,PI_IT_CODI,QUANTITAT) values ('DDF_02','GFD33','20');

-- Usuaris
Insert into USUARIS (US_USERNAME,US_PASSWORD_HASH,US_NOM_COMPLET) values ('admin','$2a$11$Wefq./ErehxD8HknLPqEF.tnHl/UxNIl486tlKW5fBjrklc7rszYW','Administrador');

COMMIT;