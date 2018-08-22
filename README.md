# SemanticParaphrasing
Generating semantically similar phrase given an input phrase.

Given a pool of semantically similar responses and an input question, pick a response that best matches the syntax of the question. A simple set difference is used to find the best matching response. The phrases are first lemmatized and stop-word cleaned before looking for matches.

The other part of this experiment involves generating a new semantically similar phrase, based on the picked response. I am using google translate for this. Translating into two other intermediate languages before translating back to english gives me the kind of paraphrasing I was looking for. However, it's far from perfect. The sophisticated way of doing this would be to use a seq2seq generator.
