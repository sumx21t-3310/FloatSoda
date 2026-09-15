import { defineCollection } from "astro:content";
import { docsLoader } from "@astrojs/starlight/loaders";
import { docsSchema } from "@astrojs/starlight/schema";

// src/content/docs/ は scripts/sync-docs.mjs が docs/ から生成する。直接編集しない
export const collections = {
  docs: defineCollection({ loader: docsLoader(), schema: docsSchema() }),
};
