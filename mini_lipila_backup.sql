--
-- PostgreSQL database dump
--

\restrict 3rrcTW5SIogj2JG2DLgsP9ZRpqjbdPEUaqp1gJzkO7aD9BVxdwVIivfUgYwUqgQ

-- Dumped from database version 16.16 (Homebrew)
-- Dumped by pg_dump version 16.16 (Homebrew)

SET statement_timeout = 0;
SET lock_timeout = 0;
SET idle_in_transaction_session_timeout = 0;
SET client_encoding = 'UTF8';
SET standard_conforming_strings = on;
SELECT pg_catalog.set_config('search_path', '', false);
SET check_function_bodies = false;
SET xmloption = content;
SET client_min_messages = warning;
SET row_security = off;

SET default_tablespace = '';

SET default_table_access_method = heap;

--
-- Name: Customers; Type: TABLE; Schema: public; Owner: davidsez
--

CREATE TABLE public."Customers" (
    "Id" uuid NOT NULL,
    "MerchantId" uuid NOT NULL,
    "Name" text NOT NULL,
    "PhoneNumber" text NOT NULL,
    "Email" text NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    "UpdatedAt" timestamp with time zone NOT NULL
);


ALTER TABLE public."Customers" OWNER TO davidsez;

--
-- Name: Merchants; Type: TABLE; Schema: public; Owner: davidsez
--

CREATE TABLE public."Merchants" (
    "Id" uuid NOT NULL,
    "AccountNumber" text NOT NULL,
    "Email" text NOT NULL,
    "Name" text DEFAULT ''::text NOT NULL,
    "CreatedAt" timestamp with time zone DEFAULT '-infinity'::timestamp with time zone NOT NULL,
    "UpdatedAt" timestamp with time zone DEFAULT '-infinity'::timestamp with time zone NOT NULL
);


ALTER TABLE public."Merchants" OWNER TO davidsez;

--
-- Name: Payments; Type: TABLE; Schema: public; Owner: davidsez
--

CREATE TABLE public."Payments" (
    "Id" uuid NOT NULL,
    "MerchantId" uuid NOT NULL,
    "WalletId" uuid NOT NULL,
    "Amount" numeric NOT NULL,
    "Status" integer NOT NULL,
    "AccountNumber" text NOT NULL,
    "Narration" text NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    "UpdatedAt" timestamp with time zone NOT NULL,
    "CustomerId" uuid,
    "Type" integer DEFAULT 0 NOT NULL
);


ALTER TABLE public."Payments" OWNER TO davidsez;

--
-- Name: Wallets; Type: TABLE; Schema: public; Owner: davidsez
--

CREATE TABLE public."Wallets" (
    "Id" uuid NOT NULL,
    "Balance" numeric NOT NULL,
    "MerchantId" uuid NOT NULL
);


ALTER TABLE public."Wallets" OWNER TO davidsez;

--
-- Name: __EFMigrationsHistory; Type: TABLE; Schema: public; Owner: davidsez
--

CREATE TABLE public."__EFMigrationsHistory" (
    "MigrationId" character varying(150) NOT NULL,
    "ProductVersion" character varying(32) NOT NULL
);


ALTER TABLE public."__EFMigrationsHistory" OWNER TO davidsez;

--
-- Data for Name: Customers; Type: TABLE DATA; Schema: public; Owner: davidsez
--

COPY public."Customers" ("Id", "MerchantId", "Name", "PhoneNumber", "Email", "CreatedAt", "UpdatedAt") FROM stdin;
71d0acd6-b089-4dfc-9bce-8f6dcd600e12	7b7c1a72-b273-495a-88bb-5e0f5b986ffb	Sezle	260979086825	testexample@gmail.com	2026-11-23 07:35:38.024949+02	2026-11-23 07:35:38.024949+02
13fd8778-86cd-4d68-a224-2bff03bfe6d4	7b7c1a72-b273-495a-88bb-5e0f5b986ffb	Mundiz	260971201987	example@gmail.com	2026-11-23 07:38:09.376442+02	2026-11-23 07:38:09.376442+02
\.


--
-- Data for Name: Merchants; Type: TABLE DATA; Schema: public; Owner: davidsez
--

COPY public."Merchants" ("Id", "AccountNumber", "Email", "Name", "CreatedAt", "UpdatedAt") FROM stdin;
1b126b07-4dd9-4a69-80f9-905b43f17dc6	0966000111	tryout@example.com		-infinity	-infinity
2e17b1ee-0922-44c6-81c3-08c753397f5f	0971111111	milupi@example.com	Milupi Store	-infinity	-infinity
452ca993-6fb1-46fd-918b-25c2516d68e5	0971111111	milupi@example.com	Milupi Store	-infinity	-infinity
f9550490-f6cf-4fb3-ac68-9cdae677cafb	0977123456	shop@example.com	Shop One	-infinity	-infinity
d62c86c5-bf2e-4870-8b74-8b7b3d408cf4	0966555444	cafe@example.com	Cafe	-infinity	-infinity
e1e93af9-62e4-4c38-875d-22e9b03213b3	0955111222	multi@example.com	Multi Store	-infinity	-infinity
c8368677-1aba-42a4-be1c-e5b496c462ba	0955111222	multi@example.com	Multi Store	-infinity	-infinity
7b7c1a72-b273-495a-88bb-5e0f5b986ffb	0979086825	shop@example.com	Shop One	-infinity	-infinity
77a9b571-3581-43fe-ad4a-105b61d5ab95	0979086734	shop@example.com	munali mall	-infinity	-infinity
839994e4-a4c4-4840-ae8a-a6f188910ffe	0979086774	shops@example.com	mandahill mall	-infinity	-infinity
cff26a75-668d-47b9-ac5f-8d2bc9b30f47	string	string	string	-infinity	-infinity
ec069474-8ab5-4042-8a04-5e131338a030	string	string	string	-infinity	-infinity
71170465-db3e-46c2-b55b-60eae99b973f	string	string	string	-infinity	-infinity
1067fb61-fe04-4643-b324-8f9fbb011d58	string	string	string	-infinity	-infinity
6ea13a61-7ac8-4c97-a9f6-9ef793f9386c	string	string	string	-infinity	-infinity
bdc68bd1-a7cd-4929-92e8-747fae4c9658	string	string	string	-infinity	-infinity
c660bf5c-4ee8-46ea-a67a-c2f9e38014fb	string	string	string	-infinity	-infinity
cc62af31-b5af-4172-b4b0-6ab402043f8f	string	string	string	-infinity	-infinity
149bca8d-d2ba-4b0c-bdcd-693895fbf337	string	string	string	-infinity	-infinity
29c99114-55a3-4bba-8b38-97736f794189	string	string	string	-infinity	-infinity
c6ee31db-e8f0-41b9-b445-429ea31a0059	0977000111	testshop@example.com	Test Shop	2026-11-22 23:55:58.425737+02	2026-11-22 23:55:58.425737+02
d3fc5bec-68e0-438e-b904-d36e7c31495e	260957061624	realme@gmail.com	Big Mupati stores	2026-11-23 13:13:28.529294+02	2026-11-23 13:13:28.529294+02
\.


--
-- Data for Name: Payments; Type: TABLE DATA; Schema: public; Owner: davidsez
--

COPY public."Payments" ("Id", "MerchantId", "WalletId", "Amount", "Status", "AccountNumber", "Narration", "CreatedAt", "UpdatedAt", "CustomerId", "Type") FROM stdin;
01a20571-c3de-7ea3-a000-6b275587cf6f	c6ee31db-e8f0-41b9-b445-429ea31a0059	144c1635-9983-40de-82ef-551a168f1ac0	1	0	260979086825		2026-11-23 00:47:51.510667+02	2026-11-23 00:47:51.510667+02	\N	0
01a2057d-f215-7418-963e-0254ef820b1f	c6ee31db-e8f0-41b9-b445-429ea31a0059	144c1635-9983-40de-82ef-551a168f1ac0	1	0	260979086825		2026-11-23 01:01:09.774205+02	2026-11-23 01:01:09.774206+02	\N	0
01a20586-3eaf-7e46-9120-12ff5306fecb	c6ee31db-e8f0-41b9-b445-429ea31a0059	144c1635-9983-40de-82ef-551a168f1ac0	1	0	260979086825		2026-11-23 01:10:13.671414+02	2026-11-23 01:10:13.671414+02	\N	0
01a2065c-8cb0-7c66-9f34-9fd3d6305046	c6ee31db-e8f0-41b9-b445-429ea31a0059	144c1635-9983-40de-82ef-551a168f1ac0	1	2	260979086825	ab	2026-11-23 05:04:18.344877+02	2026-11-23 05:04:19.547088+02	\N	0
01a20686-38b1-72e1-b1ba-9618b9ad0113	c6ee31db-e8f0-41b9-b445-429ea31a0059	144c1635-9983-40de-82ef-551a168f1ac0	1	2	0979086825	ab	2026-11-23 05:49:49.36174+02	2026-11-23 05:49:50.420745+02	\N	0
01a20589-1093-7cce-b09f-1944f8594c63	c6ee31db-e8f0-41b9-b445-429ea31a0059	144c1635-9983-40de-82ef-551a168f1ac0	1	1	260979086825	Payment to Test Shop	2026-11-23 01:13:18.47597+02	2026-11-23 06:06:56.311467+02	\N	0
01a20712-e375-7afc-aba1-1c7c8d0f1af6	7b7c1a72-b273-495a-88bb-5e0f5b986ffb	20e8c9ab-f5af-48d6-97e5-ee18a9bf6e1d	1	2	260979086825	Payout test	2026-11-23 08:23:28.082089+02	2026-11-23 08:23:29.029796+02	\N	1
01a20717-3b57-761c-99f7-fb00e36abb55	7b7c1a72-b273-495a-88bb-5e0f5b986ffb	20e8c9ab-f5af-48d6-97e5-ee18a9bf6e1d	1	2	260979086825	Payout test	2026-11-23 08:28:12.725011+02	2026-11-23 08:28:13.393157+02	\N	1
01a20721-dc5a-709a-abe0-1f487c42ba21	7b7c1a72-b273-495a-88bb-5e0f5b986ffb	20e8c9ab-f5af-48d6-97e5-ee18a9bf6e1d	1	2	260979086825	Payout test	2026-11-23 08:39:49.304017+02	2026-11-23 08:39:50.138122+02	\N	1
01a20724-0182-7c88-8e2b-50901c75ab0c	7b7c1a72-b273-495a-88bb-5e0f5b986ffb	20e8c9ab-f5af-48d6-97e5-ee18a9bf6e1d	1	2	260979086825	Payout test	2026-11-23 08:42:09.88842+02	2026-11-23 08:42:10.484865+02	\N	1
01a20725-796d-7fac-bf24-792a047b2776	7b7c1a72-b273-495a-88bb-5e0f5b986ffb	20e8c9ab-f5af-48d6-97e5-ee18a9bf6e1d	1	2	260979086825	Payout test	2026-11-23 08:43:46.153964+02	2026-11-23 08:43:46.772962+02	\N	1
01a20725-9ef5-7139-9ace-618039c3bb54	7b7c1a72-b273-495a-88bb-5e0f5b986ffb	20e8c9ab-f5af-48d6-97e5-ee18a9bf6e1d	1	2	260979086825	Payout test	2026-11-23 08:43:55.75349+02	2026-11-23 08:43:56.184136+02	\N	1
01a2072e-28b2-74db-9a58-9f5cffc9f7ee	7b7c1a72-b273-495a-88bb-5e0f5b986ffb	20e8c9ab-f5af-48d6-97e5-ee18a9bf6e1d	1	2	260979086825	Payout test	2026-11-23 08:53:15.280252+02	2026-11-23 08:53:16.215282+02	\N	1
01a20730-8c83-7e0d-b7c9-b8fa06e12639	7b7c1a72-b273-495a-88bb-5e0f5b986ffb	20e8c9ab-f5af-48d6-97e5-ee18a9bf6e1d	1	2	260979086825	Payout test	2026-11-23 08:55:51.900696+02	2026-11-23 08:55:52.580202+02	\N	1
01a20734-03f4-759a-8689-2f85fd67a0e8	7b7c1a72-b273-495a-88bb-5e0f5b986ffb	20e8c9ab-f5af-48d6-97e5-ee18a9bf6e1d	1	2	260979086825	Payout test	2026-11-23 08:59:39.090248+02	2026-11-23 08:59:40.008613+02	\N	1
01a20736-e690-78da-ad29-ceaeadb26fae	7b7c1a72-b273-495a-88bb-5e0f5b986ffb	20e8c9ab-f5af-48d6-97e5-ee18a9bf6e1d	1	2	260979086825	Payout test	2026-11-23 09:02:48.173476+02	2026-11-23 09:02:48.835272+02	\N	1
01a20739-ee5d-7e84-be54-2936f6d7003f	7b7c1a72-b273-495a-88bb-5e0f5b986ffb	20e8c9ab-f5af-48d6-97e5-ee18a9bf6e1d	1	2	260979086825	Payout test	2026-11-23 09:06:06.778725+02	2026-11-23 09:06:12.33625+02	\N	1
01a2078d-3ee6-77b3-9f44-91eee3e2d675	7b7c1a72-b273-495a-88bb-5e0f5b986ffb	20e8c9ab-f5af-48d6-97e5-ee18a9bf6e1d	1	2	260979086825	test	2026-11-23 10:37:06.881491+02	2026-11-23 10:37:07.831379+02	\N	1
01a20793-aba8-76e1-a4b7-c6687d9a2c17	7b7c1a72-b273-495a-88bb-5e0f5b986ffb	20e8c9ab-f5af-48d6-97e5-ee18a9bf6e1d	1	1	260979086825	test	2026-11-23 10:44:07.973046+02	2026-11-23 10:50:49.818928+02	\N	1
01a20700-ed25-777d-8a4a-0e255224f288	7b7c1a72-b273-495a-88bb-5e0f5b986ffb	20e8c9ab-f5af-48d6-97e5-ee18a9bf6e1d	1	2	260979086825	Customer test	2026-11-23 08:03:50.941054+02	2026-11-23 11:05:54.288909+02	71d0acd6-b089-4dfc-9bce-8f6dcd600e12	0
01a207ae-2520-75b3-9218-a5c8ef1152a2	7b7c1a72-b273-495a-88bb-5e0f5b986ffb	20e8c9ab-f5af-48d6-97e5-ee18a9bf6e1d	1	2	260979086825	test	2026-11-23 11:13:02.969366+02	2026-11-23 11:13:03.99682+02	\N	1
01a207b1-514a-7410-86e3-5365df753d86	7b7c1a72-b273-495a-88bb-5e0f5b986ffb	20e8c9ab-f5af-48d6-97e5-ee18a9bf6e1d	1	1	260979086825	test	2026-11-23 11:16:30.919801+02	2026-11-23 11:19:58.520216+02	\N	1
01a207b7-5155-7bd1-baf3-ef68c0f21ab4	7b7c1a72-b273-495a-88bb-5e0f5b986ffb	20e8c9ab-f5af-48d6-97e5-ee18a9bf6e1d	1	0	260979086825	test	2026-11-23 11:23:04.121791+02	2026-11-23 11:23:04.121791+02	\N	1
01a207b8-5d40-72dd-9faa-606304953f2d	7b7c1a72-b273-495a-88bb-5e0f5b986ffb	20e8c9ab-f5af-48d6-97e5-ee18a9bf6e1d	1	0	260979086825	test	2026-11-23 11:24:12.729576+02	2026-11-23 11:24:12.729576+02	\N	1
01a20660-9c81-7a95-b05d-1cc020e9a9ca	c6ee31db-e8f0-41b9-b445-429ea31a0059	144c1635-9983-40de-82ef-551a168f1ac0	1	2	260979086825	Test payment	2026-11-23 05:08:44.545435+02	2026-11-23 11:25:02.404795+02	\N	0
01a20660-e73a-78f6-a02c-91bee446d7a4	c6ee31db-e8f0-41b9-b445-429ea31a0059	144c1635-9983-40de-82ef-551a168f1ac0	1	2	260979086825	Test payment	2026-11-23 05:09:03.66607+02	2026-11-23 11:25:02.887211+02	\N	0
01a20662-6450-7f5d-86a0-d7a781d164ae	c6ee31db-e8f0-41b9-b445-429ea31a0059	144c1635-9983-40de-82ef-551a168f1ac0	1	2	260979086825	Test payment	2026-11-23 05:10:41.23218+02	2026-11-23 11:25:03.524331+02	\N	0
01a2074e-0087-76ca-a521-2b39e94b4258	7b7c1a72-b273-495a-88bb-5e0f5b986ffb	20e8c9ab-f5af-48d6-97e5-ee18a9bf6e1d	1	1	260979086825	Customer test	2026-11-23 09:28:02.182964+02	2026-11-23 11:25:03.933875+02	71d0acd6-b089-4dfc-9bce-8f6dcd600e12	0
01a20775-0831-76cc-8407-b8c76a27fa88	7b7c1a72-b273-495a-88bb-5e0f5b986ffb	20e8c9ab-f5af-48d6-97e5-ee18a9bf6e1d	1	2	260979086825	Customer test	2026-11-23 10:10:40.049261+02	2026-11-23 11:25:04.355941+02	71d0acd6-b089-4dfc-9bce-8f6dcd600e12	0
01a207bd-1dee-7766-80d1-4d079d48c13f	7b7c1a72-b273-495a-88bb-5e0f5b986ffb	20e8c9ab-f5af-48d6-97e5-ee18a9bf6e1d	1	2	260979086825	test	2026-11-23 11:29:24.171157+02	2026-11-23 11:29:25.227474+02	\N	1
01a207c9-8f74-73c7-83be-c8dd18085427	7b7c1a72-b273-495a-88bb-5e0f5b986ffb	20e8c9ab-f5af-48d6-97e5-ee18a9bf6e1d	1	1	260979086825	test	2026-11-23 11:42:59.667693+02	2026-11-23 11:43:18.094941+02	\N	1
01a207d0-31df-7d9c-810f-95907808da27	7b7c1a72-b273-495a-88bb-5e0f5b986ffb	20e8c9ab-f5af-48d6-97e5-ee18a9bf6e1d	1	1	260979086825	test 1	2026-11-23 11:50:14.485722+02	2026-11-23 11:51:10.447966+02	\N	0
01a207eb-5099-7378-bcb9-adb49a13b6f7	7b7c1a72-b273-495a-88bb-5e0f5b986ffb	20e8c9ab-f5af-48d6-97e5-ee18a9bf6e1d	1	2	260979086825	test	2026-11-23 12:19:51.822589+02	2026-11-23 12:19:52.2301+02	\N	1
01a207eb-b319-77db-a671-2db884d3ed8d	7b7c1a72-b273-495a-88bb-5e0f5b986ffb	20e8c9ab-f5af-48d6-97e5-ee18a9bf6e1d	1	2	260979086825	test	2026-11-23 12:20:17.030289+02	2026-11-23 12:20:17.472489+02	\N	1
01a20822-fbf4-7451-8c8b-067394b9ce72	c6ee31db-e8f0-41b9-b445-429ea31a0059	144c1635-9983-40de-82ef-551a168f1ac0	1	2	260979086825	test food	2026-11-23 13:20:40.151479+02	2026-11-23 13:20:40.617051+02	\N	1
\.


--
-- Data for Name: Wallets; Type: TABLE DATA; Schema: public; Owner: davidsez
--

COPY public."Wallets" ("Id", "Balance", "MerchantId") FROM stdin;
0381a8dd-5104-46f6-8e2a-dfbff59de006	0	e1e93af9-62e4-4c38-875d-22e9b03213b3
03bab918-747d-4a1e-ae9e-a0d4d1f98de6	250.50	e1e93af9-62e4-4c38-875d-22e9b03213b3
0dd8d355-6f20-48d4-a0c0-f97880dc2016	100	e1e93af9-62e4-4c38-875d-22e9b03213b3
2024874a-3a3f-405e-abec-be8ff7e492d5	0	2e17b1ee-0922-44c6-81c3-08c753397f5f
30226a63-a19d-41ff-aa33-c5df259e9ce9	250.550	2e17b1ee-0922-44c6-81c3-08c753397f5f
666ff78a-ee71-4fa2-9b9a-5ebd4589f5da	500	2e17b1ee-0922-44c6-81c3-08c753397f5f
20547bdb-c946-411e-93af-2227b80ca6d6	250.55	452ca993-6fb1-46fd-918b-25c2516d68e5
5d27aaa7-ed62-46c3-bda0-58f94103112f	500	452ca993-6fb1-46fd-918b-25c2516d68e5
9bda2469-e582-4009-9847-a05222caef2a	0	452ca993-6fb1-46fd-918b-25c2516d68e5
5839a3c5-30d6-4361-abee-8312874f1c07	100	c8368677-1aba-42a4-be1c-e5b496c462ba
c2ea6361-f4a8-48a0-8167-7c86c482c75d	0	c8368677-1aba-42a4-be1c-e5b496c462ba
c6b02d55-7b3c-40e1-bf42-fa65d124c05b	250.50	c8368677-1aba-42a4-be1c-e5b496c462ba
9e8c51fc-b08e-463a-866c-a34b30afacf1	100	1b126b07-4dd9-4a69-80f9-905b43f17dc6
c1e787ab-0333-4bdd-8128-a1ac71f193ca	0	1b126b07-4dd9-4a69-80f9-905b43f17dc6
d4ed3c79-4241-41ee-8284-6d13645c5f5c	250.50	1b126b07-4dd9-4a69-80f9-905b43f17dc6
7ad74354-bbe2-47bd-bae2-825a06cea9f4	270	7b7c1a72-b273-495a-88bb-5e0f5b986ffb
f617f97d-b915-4141-8ac3-3a2b9dddb2ec	289	7b7c1a72-b273-495a-88bb-5e0f5b986ffb
4008ea8a-ad50-4d6d-944d-4838c5323833	2000	77a9b571-3581-43fe-ad4a-105b61d5ab95
70b45678-496f-4a8d-909b-0be32afc5034	500	77a9b571-3581-43fe-ad4a-105b61d5ab95
cf282dac-4c55-4d6c-aeab-89d4a07f6824	100	77a9b571-3581-43fe-ad4a-105b61d5ab95
a39e3bf1-45b3-4067-b49c-e588355d0f3c	1000	839994e4-a4c4-4840-ae8a-a6f188910ffe
c18d9e0e-0afc-44d8-930c-cc4f662cd921	500	839994e4-a4c4-4840-ae8a-a6f188910ffe
ea3ac883-4557-421a-a0ce-93c14b135340	2000	839994e4-a4c4-4840-ae8a-a6f188910ffe
a1c6bb1d-74d0-4128-bd92-2ff906aa3e99	0	cff26a75-668d-47b9-ac5f-8d2bc9b30f47
10c57044-60ab-49f6-ad65-7f485a7d4e2e	0	7b7c1a72-b273-495a-88bb-5e0f5b986ffb
20e8c9ab-f5af-48d6-97e5-ee18a9bf6e1d	97	7b7c1a72-b273-495a-88bb-5e0f5b986ffb
007b866d-808d-4fc5-9836-fc573e7cf71a	0	d3fc5bec-68e0-438e-b904-d36e7c31495e
144c1635-9983-40de-82ef-551a168f1ac0	1	c6ee31db-e8f0-41b9-b445-429ea31a0059
\.


--
-- Data for Name: __EFMigrationsHistory; Type: TABLE DATA; Schema: public; Owner: davidsez
--

COPY public."__EFMigrationsHistory" ("MigrationId", "ProductVersion") FROM stdin;
20261119220747_InitialCreate	10.0.15
20261120041535_AddWallets	10.0.15
20261120073910_AddMerchantName	10.0.15
20261122215039_AddBaseEntityAndPayments	10.0.15
20261123041246_AddCustomers	10.0.15
20261123055325_AddCustomerToPayments	10.0.15
20261123061813_AddPaymentType	10.0.15
\.


--
-- Name: Customers PK_Customers; Type: CONSTRAINT; Schema: public; Owner: davidsez
--

ALTER TABLE ONLY public."Customers"
    ADD CONSTRAINT "PK_Customers" PRIMARY KEY ("Id");


--
-- Name: Merchants PK_Merchants; Type: CONSTRAINT; Schema: public; Owner: davidsez
--

ALTER TABLE ONLY public."Merchants"
    ADD CONSTRAINT "PK_Merchants" PRIMARY KEY ("Id");


--
-- Name: Payments PK_Payments; Type: CONSTRAINT; Schema: public; Owner: davidsez
--

ALTER TABLE ONLY public."Payments"
    ADD CONSTRAINT "PK_Payments" PRIMARY KEY ("Id");


--
-- Name: Wallets PK_Wallets; Type: CONSTRAINT; Schema: public; Owner: davidsez
--

ALTER TABLE ONLY public."Wallets"
    ADD CONSTRAINT "PK_Wallets" PRIMARY KEY ("Id");


--
-- Name: __EFMigrationsHistory PK___EFMigrationsHistory; Type: CONSTRAINT; Schema: public; Owner: davidsez
--

ALTER TABLE ONLY public."__EFMigrationsHistory"
    ADD CONSTRAINT "PK___EFMigrationsHistory" PRIMARY KEY ("MigrationId");


--
-- Name: IX_Customers_MerchantId; Type: INDEX; Schema: public; Owner: davidsez
--

CREATE INDEX "IX_Customers_MerchantId" ON public."Customers" USING btree ("MerchantId");


--
-- Name: IX_Wallets_MerchantId; Type: INDEX; Schema: public; Owner: davidsez
--

CREATE INDEX "IX_Wallets_MerchantId" ON public."Wallets" USING btree ("MerchantId");


--
-- Name: Customers FK_Customers_Merchants_MerchantId; Type: FK CONSTRAINT; Schema: public; Owner: davidsez
--

ALTER TABLE ONLY public."Customers"
    ADD CONSTRAINT "FK_Customers_Merchants_MerchantId" FOREIGN KEY ("MerchantId") REFERENCES public."Merchants"("Id") ON DELETE CASCADE;


--
-- Name: Wallets FK_Wallets_Merchants_MerchantId; Type: FK CONSTRAINT; Schema: public; Owner: davidsez
--

ALTER TABLE ONLY public."Wallets"
    ADD CONSTRAINT "FK_Wallets_Merchants_MerchantId" FOREIGN KEY ("MerchantId") REFERENCES public."Merchants"("Id") ON DELETE CASCADE;


--
-- PostgreSQL database dump complete
--

\unrestrict 3rrcTW5SIogj2JG2DLgsP9ZRpqjbdPEUaqp1gJzkO7aD9BVxdwVIivfUgYwUqgQ

