# SEO Content Generator

Generate SEO blog content for a website by researching real search terms and producing WordPress-importable articles.

## Inputs

The user will provide:
- **TOPIC**: The main topic of the website
- **KEYWORD**: The primary keyword to search for
- **SITE_NAME** (optional): The WordPress site name for the WXR file (defaults to "My Site")

## Part 1: Gather Search Terms

1. Use WebSearch to search Google for the provided KEYWORD.
2. From the search results, identify and collect all "People also searched for" (related searches / related queries) terms. Record how many there are (call this N).
3. For each of those N related search terms, perform a new WebSearch and collect the "People also searched for" terms from each resulting page.
4. Compile a **deduplicated master list** of all collected search terms. The expected count is approximately N + (N × M), where M is the average number of related terms per sub-search.
5. Present the full list to the user and ask for confirmation before proceeding to content generation. The user may remove or add terms.

## Part 2: Generate SEO Blog Articles

For **each search term** in the confirmed master list, generate a blog article using the following guidelines:

### Author Persona

You are an expert on **{TOPIC}** and an experienced SEO copywriter who is up-to-date on the latest SEO strategies, including considerations for AI search like GEO (Generative Engine Optimization).

### Content Tone

Conversational, clear, and scannable — prioritizing the reader's experience and providing immediate value.

### Writing Style

Mimic and blend the following writing styles:
- **Nora Roberts** — emotional resonance, vivid detail
- **Ernest Hemingway** — short, punchy sentences; clarity
- **Kurt Vonnegut Jr.** — wit, irreverence, accessible complexity
- **Jane Austen** — sharp observation, elegant structure
- **Hunter S. Thompson** — bold voice, energy, unconventional angles

### Article Structure

Each article must follow this structure:

```
Title: [SEO-optimized H1 title incorporating the search term]

TL;DR Section:
- 3–5 bullet summary answering the core question immediately

Body:
- Use H2 and H3 headings that map to likely search queries
- Answer "how," "what," and "why" questions in natural language
- Target long-tail queries related to the topic and keyword
- Include inline source attributions with links to reputable sources
  (academic, UN, government, established news outlets)
- Keep paragraphs short (2–4 sentences max)

FAQ Section:
- 4–6 frequently asked questions with concise answers
- Use questions drawn from actual related search queries
```

### Quality Requirements (Non-Negotiable)

1. The article must be **genuinely helpful, in-depth, and people-first** — not shallow keyword fluff.
2. It should **directly answer** "how," "what," and "why" questions in natural language.
3. It must be **well-structured** for both humans and search engines, with clear headings and sections that map cleanly to schema (Article with TL;DR at top and FAQ at bottom).
4. It should **target long-tail queries** related to the topic and keyword.
5. Factual statements should include **inline source attributions** with links to the original source.
6. Use only **reputable sources** such as academic sources, UN sources, and reputable news sources.

## Part 3: Create WordPress WXR Import File

After all articles are generated:

1. Compile every article into a single **WXR (WordPress eXtended RSS)** XML file.
2. Each article becomes a `<item>` with:
   - `<title>` — the article title
   - `<wp:post_type>post</wp:post_type>`
   - `<wp:status>draft</wp:status>` (so the user can review before publishing)
   - `<content:encoded>` — the full article body wrapped in `<![CDATA[...]]>`, using HTML formatting (h2, h3, p, ul, li, a tags, etc.)
   - `<wp:post_name>` — a URL-friendly slug derived from the title
   - `<category domain="category">` — set to the TOPIC
3. Use the standard WXR 1.2 namespace declarations.
4. Save the file as `seo-content-export.xml` in the project root.

## Output Summary

When complete, provide the user with:
- Total number of search terms collected
- Total number of articles generated
- Path to the WXR XML file
- Instructions for importing into WordPress: **WordPress Admin → Tools → Import → WordPress → Choose File → Upload and Import**
