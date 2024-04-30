namespace VinZ.MessageQueue;

public class VerbInvalidException(string verb) : Exception($"Verb '{verb}' is invalid.");