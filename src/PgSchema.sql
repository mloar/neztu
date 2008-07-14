--
-- PostgreSQL database dump
--

SET client_encoding = 'UTF8';
SET check_function_bodies = false;
SET client_min_messages = warning;

--
-- Name: SCHEMA public; Type: COMMENT; Schema: -; Owner: postgres
--

COMMENT ON SCHEMA public IS 'Standard public schema';


SET search_path = public, pg_catalog;

SET default_tablespace = '';

SET default_with_oids = false;

--
-- Name: History; Type: TABLE; Schema: public; Owner: www-data; Tablespace: 
--

CREATE TABLE "History" (
    "UserName" character varying(255) NOT NULL,
    "TrackId" integer NOT NULL,
    "Timestamp" timestamp without time zone NOT NULL
);


ALTER TABLE public."History" OWNER TO "www-data";

--
-- Name: ProfileData; Type: TABLE; Schema: public; Owner: www-data; Tablespace: 
--

CREATE TABLE "ProfileData" (
    "pId" character(36) NOT NULL,
    "Profile" character(36) NOT NULL,
    "Name" character varying(255) NOT NULL,
    "ValueString" text,
    "ValueBinary" bytea
);


ALTER TABLE public."ProfileData" OWNER TO "www-data";

--
-- Name: Profiles; Type: TABLE; Schema: public; Owner: www-data; Tablespace: 
--

CREATE TABLE "Profiles" (
    "pId" character(36) NOT NULL,
    "Username" character varying(255) NOT NULL,
    "ApplicationName" character varying(255) NOT NULL,
    "IsAnonymous" boolean,
    "LastActivityDate" timestamp with time zone,
    "LastUpdatedDate" timestamp with time zone
);


ALTER TABLE public."Profiles" OWNER TO "www-data";

--
-- Name: Roles; Type: TABLE; Schema: public; Owner: www-data; Tablespace: 
--

CREATE TABLE "Roles" (
    "Rolename" character varying(255) NOT NULL,
    "ApplicationName" character varying(255) NOT NULL
);


ALTER TABLE public."Roles" OWNER TO "www-data";

--
-- Name: Sessions; Type: TABLE; Schema: public; Owner: www-data; Tablespace: 
--

CREATE TABLE "Sessions" (
    "SessionId" character varying(80) NOT NULL,
    "ApplicationName" character varying(255) NOT NULL,
    "Created" timestamp with time zone NOT NULL,
    "Expires" timestamp with time zone NOT NULL,
    "Timeout" integer NOT NULL,
    "Locked" boolean NOT NULL,
    "LockId" integer NOT NULL,
    "LockDate" timestamp with time zone NOT NULL,
    "Data" text,
    "Flags" integer NOT NULL
);


ALTER TABLE public."Sessions" OWNER TO "www-data";

--
-- Name: Tracks; Type: TABLE; Schema: public; Owner: www-data; Tablespace: 
--

CREATE TABLE "Tracks" (
    "TrackId" SERIAL,
    "Filename" character varying(255),
    "Title" character varying(255) NOT NULL,
    "Artist" character varying(255) NOT NULL,
    "Album" character varying(255) NOT NULL,
    "DiscNumber" integer,
    "TrackNumber" integer,
    "Length" integer,
    "UserName" character varying(255)
);


ALTER TABLE public."Tracks" OWNER TO "www-data";

--
-- Name: Users; Type: TABLE; Schema: public; Owner: www-data; Tablespace: 
--

CREATE TABLE "Users" (
    "pId" character(36) NOT NULL,
    "Username" character varying(255) NOT NULL,
    "ApplicationName" character varying(255) NOT NULL,
    "Email" character varying(128),
    "Comment" character varying(128),
    "Password" character varying(255) NOT NULL,
    "PasswordQuestion" character varying(255),
    "PasswordAnswer" character varying(255),
    "IsApproved" boolean,
    "LastActivityDate" timestamp with time zone,
    "LastLoginDate" timestamp with time zone,
    "LastPasswordChangedDate" timestamp with time zone,
    "CreationDate" timestamp with time zone,
    "IsOnLine" boolean,
    "IsLockedOut" boolean,
    "LastLockedOutDate" timestamp with time zone,
    "FailedPasswordAttemptCount" integer,
    "FailedPasswordAttemptWindowStart" timestamp with time zone,
    "FailedPasswordAnswerAttemptCount" integer,
    "FailedPasswordAnswerAttemptWindowStart" timestamp with time zone
);


ALTER TABLE public."Users" OWNER TO "www-data";

--
-- Name: UsersInRoles; Type: TABLE; Schema: public; Owner: www-data; Tablespace: 
--

CREATE TABLE "UsersInRoles" (
    "Username" character varying(255) NOT NULL,
    "Rolename" character varying(255) NOT NULL,
    "ApplicationName" character varying(255) NOT NULL
);


ALTER TABLE public."UsersInRoles" OWNER TO "www-data";

--
-- Name: Votes; Type: TABLE; Schema: public; Owner: www-data; Tablespace: 
--

CREATE TABLE "Votes" (
    "UserName" character varying(255) NOT NULL,
    "TrackId" integer  NOT NULL,
    "Timestamp" timestamp without time zone
);


ALTER TABLE public."Votes" OWNER TO "www-data";

--
-- Name: History_pkey; Type: CONSTRAINT; Schema: public; Owner: www-data; Tablespace: 
--

ALTER TABLE ONLY "History"
    ADD CONSTRAINT "History_pkey" PRIMARY KEY ("Timestamp");


--
-- Name: Tracks_Filename_key; Type: CONSTRAINT; Schema: public; Owner: www-data; Tablespace: 
--

ALTER TABLE ONLY "Tracks"
    ADD CONSTRAINT "Tracks_Filename_key" UNIQUE ("Filename");


--
-- Name: Tracks_pkey; Type: CONSTRAINT; Schema: public; Owner: www-data; Tablespace: 
--

ALTER TABLE ONLY "Tracks"
    ADD CONSTRAINT "Tracks_pkey" PRIMARY KEY ("TrackId");


--
-- Name: Votes_pkey; Type: CONSTRAINT; Schema: public; Owner: www-data; Tablespace: 
--

ALTER TABLE ONLY "Votes"
    ADD CONSTRAINT "Votes_pkey" PRIMARY KEY ("UserName", "TrackId");


--
-- Name: profiledata_pkey; Type: CONSTRAINT; Schema: public; Owner: www-data; Tablespace: 
--

ALTER TABLE ONLY "ProfileData"
    ADD CONSTRAINT profiledata_pkey PRIMARY KEY ("pId");


--
-- Name: profiledata_profile_name_unique; Type: CONSTRAINT; Schema: public; Owner: www-data; Tablespace: 
--

ALTER TABLE ONLY "ProfileData"
    ADD CONSTRAINT profiledata_profile_name_unique UNIQUE ("Profile", "Name");


--
-- Name: profiles_pkey; Type: CONSTRAINT; Schema: public; Owner: www-data; Tablespace: 
--

ALTER TABLE ONLY "Profiles"
    ADD CONSTRAINT profiles_pkey PRIMARY KEY ("pId");


--
-- Name: profiles_username_application_unique; Type: CONSTRAINT; Schema: public; Owner: www-data; Tablespace: 
--

ALTER TABLE ONLY "Profiles"
    ADD CONSTRAINT profiles_username_application_unique UNIQUE ("Username", "ApplicationName");


--
-- Name: roles_pkey; Type: CONSTRAINT; Schema: public; Owner: www-data; Tablespace: 
--

ALTER TABLE ONLY "Roles"
    ADD CONSTRAINT roles_pkey PRIMARY KEY ("Rolename", "ApplicationName");


--
-- Name: sessions_pkey; Type: CONSTRAINT; Schema: public; Owner: www-data; Tablespace: 
--

ALTER TABLE ONLY "Sessions"
    ADD CONSTRAINT sessions_pkey PRIMARY KEY ("SessionId", "ApplicationName");


--
-- Name: users_pkey; Type: CONSTRAINT; Schema: public; Owner: www-data; Tablespace: 
--

ALTER TABLE ONLY "Users"
    ADD CONSTRAINT users_pkey PRIMARY KEY ("pId");


--
-- Name: users_username_application_unique; Type: CONSTRAINT; Schema: public; Owner: www-data; Tablespace: 
--

ALTER TABLE ONLY "Users"
    ADD CONSTRAINT users_username_application_unique UNIQUE ("Username", "ApplicationName");


--
-- Name: usersinroles_pkey; Type: CONSTRAINT; Schema: public; Owner: www-data; Tablespace: 
--

ALTER TABLE ONLY "UsersInRoles"
    ADD CONSTRAINT usersinroles_pkey PRIMARY KEY ("Username", "Rolename", "ApplicationName");


--
-- Name: profiles_isanonymous_index; Type: INDEX; Schema: public; Owner: www-data; Tablespace: 
--

CREATE INDEX profiles_isanonymous_index ON "Profiles" USING btree ("IsAnonymous");


--
-- Name: users_email_index; Type: INDEX; Schema: public; Owner: www-data; Tablespace: 
--

CREATE INDEX users_email_index ON "Users" USING btree ("Email");


--
-- Name: users_islockedout_index; Type: INDEX; Schema: public; Owner: www-data; Tablespace: 
--

CREATE INDEX users_islockedout_index ON "Users" USING btree ("IsLockedOut");


--
-- Name: profiledata_profile_fkey; Type: FK CONSTRAINT; Schema: public; Owner: www-data
--

ALTER TABLE ONLY "ProfileData"
    ADD CONSTRAINT profiledata_profile_fkey FOREIGN KEY ("Profile") REFERENCES "Profiles"("pId") ON DELETE CASCADE;


--
-- Name: profiles_username_fkey; Type: FK CONSTRAINT; Schema: public; Owner: www-data
--

ALTER TABLE ONLY "Profiles"
    ADD CONSTRAINT profiles_username_fkey FOREIGN KEY ("Username", "ApplicationName") REFERENCES "Users"("Username", "ApplicationName") ON DELETE CASCADE;


--
-- Name: usersinroles_rolename_fkey; Type: FK CONSTRAINT; Schema: public; Owner: www-data
--

ALTER TABLE ONLY "UsersInRoles"
    ADD CONSTRAINT usersinroles_rolename_fkey FOREIGN KEY ("Rolename", "ApplicationName") REFERENCES "Roles"("Rolename", "ApplicationName") ON DELETE CASCADE;


--
-- Name: usersinroles_username_fkey; Type: FK CONSTRAINT; Schema: public; Owner: www-data
--

ALTER TABLE ONLY "UsersInRoles"
    ADD CONSTRAINT usersinroles_username_fkey FOREIGN KEY ("Username", "ApplicationName") REFERENCES "Users"("Username", "ApplicationName") ON DELETE CASCADE;


--
-- Name: public; Type: ACL; Schema: -; Owner: postgres
--

REVOKE ALL ON SCHEMA public FROM PUBLIC;
REVOKE ALL ON SCHEMA public FROM postgres;
GRANT ALL ON SCHEMA public TO postgres;
GRANT ALL ON SCHEMA public TO PUBLIC;


--
-- Name: History; Type: ACL; Schema: public; Owner: www-data
--

REVOKE ALL ON TABLE "History" FROM PUBLIC;
REVOKE ALL ON TABLE "History" FROM "www-data";
GRANT ALL ON TABLE "History" TO "www-data";


--
-- Name: Tracks; Type: ACL; Schema: public; Owner: www-data
--

REVOKE ALL ON TABLE "Tracks" FROM PUBLIC;
REVOKE ALL ON TABLE "Tracks" FROM "www-data";
GRANT ALL ON TABLE "Tracks" TO "www-data";


--
-- Name: Votes; Type: ACL; Schema: public; Owner: www-data
--

REVOKE ALL ON TABLE "Votes" FROM PUBLIC;
REVOKE ALL ON TABLE "Votes" FROM "www-data";
GRANT ALL ON TABLE "Votes" TO "www-data";


--
-- PostgreSQL database dump complete
--

