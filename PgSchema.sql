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
-- Name: History; Type: TABLE; Schema: public; Owner: neztu; Tablespace: 
--

CREATE TABLE "History" (
    "UserName" character varying(255) NOT NULL,
    "TrackId" integer NOT NULL,
    "Timestamp" timestamp without time zone NOT NULL DEFAULT now()
);


ALTER TABLE public."History" OWNER TO "neztu";

--
-- Name: Tracks; Type: TABLE; Schema: public; Owner: neztu; Tablespace: 
--

CREATE TABLE "Tracks" (
    "TrackId" SERIAL,
    "Filename" character varying(255) NOT NULL,
    "Title" character varying(255) NOT NULL,
    "Artist" character varying(255) NOT NULL,
    "Album" character varying(255) NOT NULL,
    "Genre" character varying(255) NOT NULL,
    "DiscNumber" integer,
    "TrackNumber" integer,
    "Length" integer,
    "Uploader" character varying(255) NOT NULL
);


ALTER TABLE public."Tracks" OWNER TO "neztu";

--
-- Name: Votes; Type: TABLE; Schema: public; Owner: neztu; Tablespace: 
--

CREATE TABLE "Votes" (
    "UserName" character varying(255) NOT NULL,
    "TrackId" integer  NOT NULL,
    "Timestamp" timestamp without time zone NOT NULL DEFAULT now()
);


ALTER TABLE public."Votes" OWNER TO "neztu";

--
-- Name: History_pkey; Type: CONSTRAINT; Schema: public; Owner: neztu; Tablespace: 
--

ALTER TABLE ONLY "History"
    ADD CONSTRAINT "History_pkey" PRIMARY KEY ("Timestamp");


--
-- Name: Tracks_Filename_key; Type: CONSTRAINT; Schema: public; Owner: neztu; Tablespace: 
--

ALTER TABLE ONLY "Tracks"
    ADD CONSTRAINT "Tracks_Filename_key" UNIQUE ("Filename");


--
-- Name: Tracks_pkey; Type: CONSTRAINT; Schema: public; Owner: neztu; Tablespace: 
--

ALTER TABLE ONLY "Tracks"
    ADD CONSTRAINT "Tracks_pkey" PRIMARY KEY ("TrackId");


--
-- Name: Votes_pkey; Type: CONSTRAINT; Schema: public; Owner: neztu; Tablespace: 
--

ALTER TABLE ONLY "Votes"
    ADD CONSTRAINT "Votes_pkey" PRIMARY KEY ("UserName", "TrackId");


--
-- Name: tracks_title_index; Type: INDEX; Schema: public; Owner: neztu; Tablespace: 
--

CREATE INDEX tracks_title_index ON "Tracks" USING btree ("Title");


--
-- Name: tracks_artist_index; Type: INDEX; Schema: public; Owner: neztu; Tablespace: 
--

CREATE INDEX tracks_artist_index ON "Tracks" USING btree ("Artist");


--
-- Name: tracks_album_index; Type: INDEX; Schema: public; Owner: neztu; Tablespace: 
--

CREATE INDEX tracks_album_index ON "Tracks" USING btree ("Album");


--
-- Name: public; Type: ACL; Schema: -; Owner: postgres
--

REVOKE ALL ON SCHEMA public FROM PUBLIC;
REVOKE ALL ON SCHEMA public FROM postgres;
GRANT ALL ON SCHEMA public TO postgres;
GRANT ALL ON SCHEMA public TO PUBLIC;


--
-- Name: History; Type: ACL; Schema: public; Owner: neztu
--

REVOKE ALL ON TABLE "History" FROM PUBLIC;
REVOKE ALL ON TABLE "History" FROM "neztu";
GRANT ALL ON TABLE "History" TO "neztu";


--
-- Name: Tracks; Type: ACL; Schema: public; Owner: neztu
--

REVOKE ALL ON TABLE "Tracks" FROM PUBLIC;
REVOKE ALL ON TABLE "Tracks" FROM "neztu";
GRANT ALL ON TABLE "Tracks" TO "neztu";


--
-- Name: Votes; Type: ACL; Schema: public; Owner: neztu
--

REVOKE ALL ON TABLE "Votes" FROM PUBLIC;
REVOKE ALL ON TABLE "Votes" FROM "neztu";
GRANT ALL ON TABLE "Votes" TO "neztu";


--
-- PostgreSQL database dump complete
--

